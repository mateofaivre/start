using ClosedXML.Excel;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Start.Core.Entities;
using Start.Core.Readers;
using Start.Core.Responses;
using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Start.Core.Services
{
    public class ExportService
    {
        private readonly AzureStorageService _azureStorageService;
        private readonly Repositories.OeuvreRepository _oeuvreRepository;
        private readonly ILogger _logger;

        public ExportService(
            ILogger logger,
            Repositories.OeuvreRepository oeuvreRepository,
            AzureStorageService azureStorageService)
        {
            _logger = logger;
            _oeuvreRepository = oeuvreRepository;
            _azureStorageService = azureStorageService;
        }

        public async Task<ExportResponse> Backup()
        {
            var oeuvreResult = await _oeuvreRepository.FindAllOeuvre();
            var kml = KmlReader.CreateKml(oeuvreResult.Valid.Features);
            var response = new ExportResponse();

            var globalResult = new SearchGlobalQueryResult()
            {
                Deleted = oeuvreResult.Deleted,
                New = oeuvreResult.New,
                Valid = oeuvreResult.Valid,
                Waiting = oeuvreResult.Waiting,
                Approved = oeuvreResult.Approved,
            };

            using (var stream = new MemoryStream())
            {
                kml.Save(stream);
                stream.Seek(0, SeekOrigin.Begin);

                var kmlBlobUri = await _azureStorageService.UploadBlob("backup", "st_art_backup.kml", stream);
                response.KmlFileUri = kmlBlobUri.ToString();
            }

            var json = JsonConvert.SerializeObject(globalResult);
            using (var jsonStream = new MemoryStream())
            {
                using (var sw = new StreamWriter(jsonStream, Encoding.UTF8))
                {
                    sw.Write(json);
                    jsonStream.Seek(0, SeekOrigin.Begin);

                    var jsonBlobUri = await _azureStorageService.UploadBlob("backup", "st_art_backup.json", jsonStream);
                    response.JsonFileUri = jsonBlobUri.ToString();
                }
            }

            return response;
        }

        public async Task<FileResponse> ExportNews()
        {
            var response = new FileResponse();

            var oeuvreResult = await _oeuvreRepository.FindAllOeuvreByStatus(OeuvreStatus.Valid);

            using var workbook = new XLWorkbook();
            var worksheet = workbook.Worksheets.Add("Nouveautés");
            var currentRow = 1;

            worksheet.Cell(currentRow, 1).Value = "Marqueur";
            worksheet.Cell(currentRow, 2).Value = "Pseudo";
            worksheet.Cell(currentRow, 3).Value = "Email";
            worksheet.Cell(currentRow, 4).Value = "Url";
            worksheet.Cell(currentRow, 5).Value = "Artiste";

            foreach (var oeuvre in oeuvreResult.New.Features.Select(f => f.Properties))
            {
                currentRow++;

                worksheet.Cell(currentRow, 1).Value = oeuvre.Marqueur;
                worksheet.Cell(currentRow, 2).Value = oeuvre.UtilisateurPseudo;
                worksheet.Cell(currentRow, 3).Value = oeuvre.UtilisateurEmail;
                worksheet.Cell(currentRow, 4).Value = oeuvre.ImageUrl.Replace("/br/", "/hd/") + ".jpg";
                worksheet.Cell(currentRow, 5).Value = oeuvre.Artiste;
            }

            using (var stream = new MemoryStream())
            {
                workbook.SaveAs(stream);
                stream.Seek(0, SeekOrigin.Begin);

                var newBlobUri = await _azureStorageService.UploadBlob("exportnews", $"news-{DateTime.Now:ddMMyyyy}.xlsx", stream);
                response.FileUri = newBlobUri.ToString();
            }

            return response;
        }
    }
}
