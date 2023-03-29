using Azure.Maps.Search.Models;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Start.Core.Entities;
using Start.Core.Repositories;
using Start.Core.Requests;
using Start.Core.Services;
using System;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using System.Web;

namespace Start.Core.Process
{
    public class UpdateOeuvreProcess
    {
        private readonly ILogger _logger;
        private readonly OeuvreRepository _oeuvreRepository;
        private readonly AzureStorageService _azureStorageService;
        public UpdateOeuvreProcess(
            ILogger logger,
            OeuvreRepository oeuvreRepository,
            AzureStorageService azureStorageService)
        {
            _logger = logger;
            _oeuvreRepository = oeuvreRepository;
            _azureStorageService = azureStorageService;
        }

        public async Task<Oeuvre> Excecute(UpdateOeuvreRequest request)
        {
            var newMarqueur = Guid.NewGuid().ToString();
            var oldOeuvre = request.FindWithLocation
                ? await _oeuvreRepository.FindOeuvre(request.Location)
                : await _oeuvreRepository.FindOeuvre(request.IdOeuvre);

            //TODO : Ajouter la vérification de l'adresse avec la position.
            //Si aucune position valide, ne pas insérer l'image
            //Mettre à jour les valeurs de l'adresse avec celles touvées depuis la géolocalisation
            //Vérifier que l'adresse enregistrée est freeformAddress et qu'elle est bien renvoyée 

            
            var newOeuvre = new Oeuvre
            {
                PartitionKey = "item",
                Status = (int)OeuvreStatus.WaitingValidation,
                Adresse = request.Adresse,
                Artiste = request.Artiste,
                DateCreation = DateTime.Now,
                Informations = request.Informations,
                Location = request.Location,
                Marqueur = newMarqueur,
                Pays = request.Pays,
                Rue = request.Rue,
                TypeOeuvre = request.TypeOeuvre,
                UtilisateurId = request.UserId,
                UtilisateurEmail = request.UserMail,
                UtilisateurPseudo = request.UserPseudo,
                Ville = request.Ville
            };

            var fileStream =  GetStreamFromRawData(request);
            var fileName = oldOeuvre?.ImageName;
            
            if (fileStream != null)
            {
                var fileNameId = oldOeuvre == null
                  ? newMarqueur
                  : $"{DateTime.Now.Ticks} - {oldOeuvre.Marqueur}";

                fileName = $"{fileNameId}.jpg";

                var dateCreation = /*request.ImageDate ??*/ DateTime.Now;
                var folderDate = dateCreation.ToString("yyyyMM");

                var (uploadSucceeded, brUri, hdUri, thumbnailUri, isLandscape) = await UploadPhoto(fileStream, folderDate, fileName);

                if (uploadSucceeded)
                {
                    newOeuvre.DateCreation = dateCreation;
                    newOeuvre.ImageUrl = hdUri.ToString();
                    newOeuvre.ImageBrUrl = brUri.ToString();
                    newOeuvre.ImageThumbnailUrl = thumbnailUri.ToString();
                    newOeuvre.IsLandscape = isLandscape;
                }
                else
                {
                    return null;
                }
            }

            newOeuvre.ImageName = fileName;
            newOeuvre.ImageBase64 = "";
            

            //Update and move current feature
            if (oldOeuvre != null)
            {
                newOeuvre.UpdateValues(oldOeuvre, fileStream != null);
                oldOeuvre.PartitionKey = OeuvrePartitionKeys.Updating;

                await _oeuvreRepository.InsertOrMergeAsync(oldOeuvre);
            }

            //Update or insert feature
            await _oeuvreRepository.InsertOrMergeAsync(newOeuvre);

            //Approve update when is request from an admin
            if (request.IsAdmin)
            {
                var approbationProcess = new ApprobationProcess(_logger, _oeuvreRepository);
                await approbationProcess.Exceute(new ApproveRequest
                {
                    Approved = true,
                    OeuvreId = newOeuvre.Marqueur
                });
            }

            return await _oeuvreRepository.FindOeuvre(newOeuvre.Marqueur);
        }

        private Stream GetStreamFromRawData(UpdateOeuvreRequest request)
        {
            var rawData = request.RawData;
            var rawBinary = request.RawBinary;
            byte[] fileArray = null;

            if (!string.IsNullOrEmpty(rawData) && rawData.StartsWith("data"))
            {
                rawData = rawData.Split(new char[] { ',' })[1];
            }

            fileArray = !string.IsNullOrEmpty(rawData)
                   ? Convert.FromBase64String(rawData)
                   : rawBinary != null
                    ? rawBinary
                    : null;

            var stream = fileArray != null
                ? new MemoryStream(fileArray)
                : null;

            return stream;
        }

        private async Task<(bool uploadSucceeded, Uri brUri, Uri hdUri, Uri thumbnailUri, bool isLandscape)> UploadPhoto(Stream fileStream, string folderDate, string fileName)
        {
            var uploadSucceeded = false;
            Uri brUri = null;
            Uri hdUri = null; 
            Uri thumbnailUri = null;
            bool isLandscape = false;

            try
            {
                var photoBr = ImageResizeService.GenerateNew(fileStream, 0.4f);
                var brBlobName = $"br/{folderDate}/{fileName}";
                brUri = await _azureStorageService.UploadBlob("photos", brBlobName, photoBr);

                fileStream.Seek(0, SeekOrigin.Begin);
                var photoHd = ImageResizeService.GenerateNew(fileStream);
                var hdBlobName = $"hd/{folderDate}/{fileName}";
                hdUri = await _azureStorageService.UploadBlob("photos", hdBlobName, photoHd);

                fileStream.Seek(0, SeekOrigin.Begin);
                var photoThumbnail = ImageResizeService.GenerateNew(fileStream, 150);
                var thumbnailBlobName = $"thumbnail/{folderDate}/{fileName}";
                thumbnailUri = await _azureStorageService.UploadBlob("photos", thumbnailBlobName, photoThumbnail);

                isLandscape = ImageResizeService.IsLandscape(photoThumbnail);

                uploadSucceeded = true;
            }
            catch(Exception e)
            {
                _logger.LogError(e.Message);
            }

            return (uploadSucceeded, brUri, hdUri, thumbnailUri, isLandscape);
        }

    }
}
