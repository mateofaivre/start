using ClosedXML.Excel;
using Microsoft.Azure.Cosmos.Spatial;
using Microsoft.Extensions.Logging;
using Start.Core.Entities;
using Start.Core.Repositories;
using Start.Core.Requests;
using Start.Core.Responses;
using Start.Core.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Start.Core.Process
{
    public class ImportProcess
    {
        private readonly ILogger _logger;
        private readonly OeuvreRepository _oeuvreRepository;
        private readonly AzureStorageService _azureStorageService;
        private readonly AzureMapService _azureMapService;
        private readonly ImageExifService _imageExifService;

        private Dictionary<string, (Oeuvre oeuvre, byte[] image)> _oeuvreInZip;
        private Dictionary<string, string> _oeuvreInErrorInZip;
        private Dictionary<string, Oeuvre> _oeuvreImported;
        private Dictionary<Oeuvre, byte[]> _oeuvreToImport;
        private Dictionary<string, byte[]> _filesInZip;

        public ImportProcess(
            ILogger logger,
            OeuvreRepository oeuvreRepository,
            AzureStorageService azureStorageService,
            AzureMapService azureMapService)
        {
            _logger = logger;
            _oeuvreRepository = oeuvreRepository;
            _azureStorageService = azureStorageService;
            _azureMapService = azureMapService;
            _imageExifService = new ImageExifService();

            _oeuvreInZip = new Dictionary<string, (Oeuvre oeuvre, byte[] image)>();
            _oeuvreInErrorInZip = new Dictionary<string, string>();
            _oeuvreImported = new Dictionary<string, Oeuvre>();
            _oeuvreToImport = new Dictionary<Oeuvre, byte[]>();
            _filesInZip = new Dictionary<string, byte[]>();
        }


        public async Task<ImportResponse> Execute(ImportRequest importRequest)
        {
            var response = new ImportResponse();
            response.Status = ImportResponseStatus.Error;

            //1-vérification données valides (Pseudo, File not empty)
            if (importRequest == null)
            {
                var message = "Demande invalide - aucune donnée";

                _logger.LogError(message);
                response.Message = message;

                return response;
            }

            if (importRequest.Utilisateur == null)
            {
                var message = "Demande invalide - utilisateur non défini";

                _logger.LogError(message);
                response.Message = message;

                return response;
            }

            if (string.IsNullOrEmpty(importRequest.Utilisateur.Id))
            {
                var message = "Demande invalide - l'identifiant de l'utilisateur n'est pas défini";

                _logger.LogError(message);
                response.Message = message;

                return response;
            }


            //2-Ouverture du zip :
            //Récupération de la liste des oeuvres à traiter et du fichier associé
            try
            {
                var finished = await ExtractData(importRequest.ZipFile);
                if (!finished)
                {
                    var message = "Aucune donnée extraite";

                    _logger.LogInformation(message);
                    response.Message = message;
                    response.Status = ImportResponseStatus.Empty;

                    return response;
                }
            }
            catch(Exception e)
            {
                var message = $"Erreur lors de l'extraction {e.Message}";

                _logger.LogError(message);
                response.Message = message;
                
                return response;
            }

            //3-pour chaque oeuvre valide (un fichier, une localisation (exif/coordonnées/adresse)
            await ValidateData();

            //4-Import des données via un appel au process d'Update

            foreach(var item in _oeuvreToImport)
            {
                var oeuvre = item.Key;
                var dateImage = ImageResizeService.GetDateCreation(new MemoryStream(item.Value));

                var updateProcess = new UpdateOeuvreProcess(_logger, _oeuvreRepository, _azureStorageService);
                var updateRequest = new UpdateOeuvreRequest
                {
                    Adresse = oeuvre.Adresse,
                    Artiste = oeuvre.Artiste,
                    ImageDate = dateImage,
                    Informations = oeuvre.Informations,
                    Location = new Point(new Position(oeuvre.Location.Position.Latitude, oeuvre.Location.Position.Longitude)), //Intervertion des lontitude et longitudes pour sauvegarder au format geojson qui prend toujours la lontidude puis la latitude
                    RawBinary = item.Value,
                    Rue = oeuvre.Rue,
                    TypeOeuvre = oeuvre.TypeOeuvre,
                    UserId = importRequest.Utilisateur.Id,
                    UserMail = importRequest.Utilisateur.Email,
                    UserPseudo = importRequest.Utilisateur.Pseudo,
                    Ville = oeuvre.Ville,
                    Pays = oeuvre.Pays,
                    FindWithLocation = true,
                };

                var result = await updateProcess.Excecute(updateRequest);
                if (result != null)
                {
                    //Ajouter aux oeuvres importées avec succès
                    _oeuvreImported.Add(oeuvre.ImageName, result);
                }
                else
                {
                    //Ajouter aux oeuvres en erreur lors de l'envoi
                    _oeuvreInErrorInZip.Add(oeuvre.ImageName, "Erreur de la sauvegarde de l'oeuvre dans le tenant");
                }
            }

            response.OeuvreImported = _oeuvreImported.Values.ToList();
            response.Status = _oeuvreInErrorInZip.Count > 0 ? ImportResponseStatus.Partial : ImportResponseStatus.Succeeded;
            response.Message = _oeuvreInErrorInZip.Count > 0 
                ? "Import partiellement réussi, veuillez consulter le fichier de rapport"
                : "Import réussi avec succès";

            //4-rapport d'import (Liste des oeuvre importées, zip des données en erreur (liste oeuvre - raison erreur / Photo)
            //Générer un zip contenant les données source et nouveau fichier de report avec les infos sur les données importées, en erreur et la raison
            response.Report = GenerateReportFile();

            return response;
        }

        private async Task<bool> ExtractData(byte[] zipFile)
        {
            //1-Unzip 
            _filesInZip = await Helpers.ZipHelper.UnZip(zipFile);

            if (_filesInZip.Values.Any())
            {
                //2-recherche du fichier xls
                var excelFile = _filesInZip.FirstOrDefault(f => f.Key == "ImportOeuvre.xlsx").Value;
                if(excelFile == null)
                {
                    _logger.LogError("Traitement du zip - Aucun fichier excel trouvé");
                    return false;
                }

                //3-génération de la liste des oeuvres décrites dans le fichier
                ReadExcelFile(excelFile);

                //3-Recherche de chaque image associée à une entrée dans le fichier excel
                foreach(var oeuvreImageName in _oeuvreInZip.Keys.ToList())
                {
                    (Oeuvre oeuvre, byte[] image) oeuvreInfos = _oeuvreInZip[oeuvreImageName];
                    oeuvreInfos.image = _filesInZip.FirstOrDefault(f => f.Key.StartsWith(oeuvreImageName)).Value;
                    _oeuvreInZip[oeuvreImageName] = oeuvreInfos;
                }
            }
            else
            {
                return false;
            }

            return true;
        }

        private void ReadExcelFile(byte[] excelFile)
        {
            var memoryFile = new MemoryStream(excelFile);
            memoryFile.Seek(0, SeekOrigin.Begin);

            using var workbook = new XLWorkbook(memoryFile);
            
            var workSheet = workbook.Worksheets.FirstOrDefault();
            
            foreach(var row in workSheet.RowsUsed().Skip(1))
            {
                var oeuvre = new Oeuvre();
                oeuvre.ImageName =  row.Cell(1).GetString();
                oeuvre.Artiste = row.Cell(2).GetString();
                oeuvre.TypeOeuvre = row.Cell(3).GetString();
                oeuvre.Informations = row.Cell(4).GetString();
                oeuvre.Pays = row.Cell(5).GetString();
                oeuvre.Ville = row.Cell(6).GetString();
                oeuvre.Rue = row.Cell(7).GetString();

                var latitude = Helpers.ParserHelper.ParseDouble(row.Cell(8).GetString());
                var longitude = Helpers.ParserHelper.ParseDouble(row.Cell(9).GetString());

                if (latitude != 0 && longitude != 0)
                {
                    oeuvre.Location = new Point(longitude, latitude);
                }

                _oeuvreInZip.Add(oeuvre.ImageName, (oeuvre, null));
            }
        }

        private async Task ValidateData()
        {
            foreach(var imageName in _oeuvreInZip.Keys)
            {
                (Oeuvre oeuvre, byte[] file) = _oeuvreInZip[imageName];
                if (string.IsNullOrEmpty(oeuvre.ImageName))
                {
                    _logger.LogInformation("Import validation oeuvre, aucun fichier n'est spécifié pour une oeuvre");
                    _oeuvreInErrorInZip.Add(oeuvre.ImageName, "Aucun fichier n'est spécifié pour cette oeuvre");
                    continue;
                }

                if (file == null)
                {
                    _logger.LogInformation($"Import validation oeuvre, aucun fichier trouvé pour {oeuvre.ImageName}");
                    _oeuvreInErrorInZip.Add(imageName, $"Aucun fichier trouvé pour {oeuvre.ImageName}");
                    continue;
                }

                var location = _imageExifService.GetImageGeolocalisation(new MemoryStream(file));
                if (location != null)
                {
                    oeuvre.Location = location;
                }

                if (oeuvre.Location != null)
                {
                    var adresse = await _azureMapService.GetAddressAsync(oeuvre.Location);
                    if(adresse != null)
                    {
                        oeuvre.Adresse = adresse.FreeformAddress;
                        oeuvre.Pays = adresse.Pays;
                        oeuvre.Ville = adresse.Ville;
                        oeuvre.Rue = adresse.Rue;
                    }
                }

                var oeuvreAdresse = new Adresse(oeuvre.Pays, oeuvre.Ville, oeuvre.Rue);
                if (oeuvre.Location == null && oeuvreAdresse.IsComplete)
                {
                    oeuvre.Adresse = oeuvreAdresse.ToString();
                    oeuvre.Location = await _azureMapService.GetPositionAsync(oeuvreAdresse);
                }

                if (oeuvre.Location == null && !oeuvreAdresse.IsComplete)
                {
                    _logger.LogInformation($"Import validation oeuvre, aucune adresse ou position n'est définie dans l'image ou le fichier excel pour cette oeuvre {oeuvre.ImageName}");
                    _oeuvreInErrorInZip.Add(imageName, "Aucune adresse ou position n'est définie dans l'image ou le fichier excel pour cette oeuvre");
                    continue;
                }

                if (!string.IsNullOrEmpty(oeuvre.TypeOeuvre) &&
                        oeuvre.TypeOeuvre != TypeOeuvres.Autre &&
                        oeuvre.TypeOeuvre != TypeOeuvres.Collage &&
                        oeuvre.TypeOeuvre != TypeOeuvres.Fresque &&
                        oeuvre.TypeOeuvre != TypeOeuvres.Graffiti &&
                        oeuvre.TypeOeuvre != TypeOeuvres.Mosaique &&
                        oeuvre.TypeOeuvre != TypeOeuvres.Pochoir)
                {
                    oeuvre.TypeOeuvre = TypeOeuvres.Autre;
                }

                _oeuvreToImport.Add(oeuvre, file);
            }
        }

        private byte[] GenerateReportFile()
        {
            byte[] excelReport = null;
            using var workbook = new XLWorkbook();
            var worksheet = workbook.Worksheets.Add("Nouveautés");
            var currentRow = 1;

            worksheet.Cell(currentRow, 1).Value = "ImageName";
            worksheet.Cell(currentRow, 2).Value = "Marqueur";
            worksheet.Cell(currentRow, 3).Value = "Importée";
            worksheet.Cell(currentRow, 4).Value = "Erreur";

            foreach (var imageName in _oeuvreImported.Keys)
            {
                currentRow++;

                var oeuvre = _oeuvreImported[imageName];
                worksheet.Cell(currentRow, 1).Value = imageName;
                worksheet.Cell(currentRow, 2).Value = oeuvre.Marqueur;
                worksheet.Cell(currentRow, 3).Value = "Oui";
            }

            foreach (var imageName in _oeuvreInErrorInZip.Keys)
            {
                currentRow++;

                var error = _oeuvreInErrorInZip[imageName];
                worksheet.Cell(currentRow, 1).Value = imageName;
                worksheet.Cell(currentRow, 2).Value = "";
                worksheet.Cell(currentRow, 3).Value = "Non";
                worksheet.Cell(currentRow, 4).Value = error;
            }

            using (var stream = new MemoryStream())
            {
                workbook.SaveAs(stream);
                stream.Seek(0, SeekOrigin.Begin);

                excelReport = stream.ToArray();
            }

            return Helpers.ZipHelper.Zip(new Dictionary<string, byte[]>(_filesInZip)
            {
                {"report.xlsx", excelReport},
            });
        }
    }
}
