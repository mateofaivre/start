using Azure.Storage.Blobs;
using ClosedXML.Excel;
using Microsoft.Extensions.Logging;
using Start.Core.DataSourceContexts;
using Start.Core.Entities;
using Start.Core.Helpers;
using Start.Core.Repositories;
using Start.Core.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Start.Core.Process
{
    public class ExportProcess
    {
        private readonly CosmosDbContext _sourceCosmosDbContext;
        private readonly OeuvreRepository _sourceOeuvreRepository;
        private readonly AzureStorageService _azureStorageService;
        private readonly ILogger<ExportProcess> _logger;

        public ExportProcess(ILoggerFactory loggerFactory)
        {
            _sourceCosmosDbContext = new CosmosDbContext(new DestinationCosmosDbConfigOptions(), loggerFactory.CreateLogger<CosmosDbContext>());
            _sourceOeuvreRepository = new OeuvreRepository(_sourceCosmosDbContext, loggerFactory.CreateLogger<OeuvreRepository>());
            _azureStorageService = new AzureStorageService(new AzureStorageConfigOptions(), loggerFactory.CreateLogger<AzureStorageService>());
            _logger = loggerFactory.CreateLogger<ExportProcess>();
        }

        public async Task Execute()
        {
            var oeuvreByType = new Dictionary<string, Dictionary<string, byte[]>>();
            
            //Get oeuvre in database with specific filter
            string query = "SELECT * FROM c where c.ville like '%Dijon%' and c.status = 0";
            var oeuvreResult = await _sourceOeuvreRepository.FindAllOeuvreWithQuery(query);

            foreach (var oeuvreType in oeuvreResult.GroupBy(o => o.TypeOeuvre))
            {
                var dictionnary = new Dictionary<string, byte[]>();

                foreach (var oeuvre in oeuvreType)
                {
                    //Get blob file for each oeuvre
                    var brBlobName = $"br/{oeuvre.DateCreation:yyyyMM}/{oeuvre.ImageName}";
                    var blobclient = _azureStorageService.GetBlob("photos", brBlobName);

                    if(await blobclient.ExistsAsync())
                    {
                        var content = blobclient.DownloadContent().Value.Content.ToMemory().ToArray();
                        dictionnary.Add(oeuvre.ImageName, content);
                    }
                }

                oeuvreByType.Add(oeuvreType.Key, dictionnary);
            }

            //Generate xls file with column : Current file id, artiste name, street, city, state, type, description, publication date
            var excelStream = GenerateExcel(oeuvreResult);

            using(var file = new FileStream("c:/oeuvre.xlsx", FileMode.Create, FileAccess.Write))
            {
                excelStream.CopyTo(file);
            }

            //Generate zip file with folder by type d'oeuvre (in next version try to take this kind of grouping)
            var zipArray = ZipHelper.ZipWithDirectory(oeuvreByType);

            var ms = new MemoryStream(zipArray);
            ms.Seek(0, SeekOrigin.Begin);
            using (var file = new FileStream("c:/oeuvre.zip", FileMode.Create, FileAccess.Write))
            {
                ms.CopyTo(file);
            }
        }

        private Stream GenerateExcel(List<Oeuvre> oeuvres)
        {
            using var workbook = new XLWorkbook();
            var worksheet = workbook.Worksheets.Add("Oeuvres");
            var currentRow = 1;

            worksheet.Cell(currentRow, 1).Value = "Marqueur";
            worksheet.Cell(currentRow, 2).Value = "Nom du fichier";
            worksheet.Cell(currentRow, 3).Value = "Artiste";
            worksheet.Cell(currentRow, 4).Value = "Rue";
            worksheet.Cell(currentRow, 5).Value = "Ville";
            worksheet.Cell(currentRow, 6).Value = "Gps";
            worksheet.Cell(currentRow, 7).Value = "Type oeuvre";
            worksheet.Cell(currentRow, 8).Value = "Description";
            worksheet.Cell(currentRow, 9).Value = "Date publication";

            foreach (var oeuvre in oeuvres)
            {
                currentRow++;

                worksheet.Cell(currentRow, 1).Value = oeuvre.Marqueur;
                worksheet.Cell(currentRow, 2).Value = oeuvre.ImageName;
                worksheet.Cell(currentRow, 3).Value = oeuvre.Artiste;
                worksheet.Cell(currentRow, 4).Value = oeuvre.Rue;
                worksheet.Cell(currentRow, 5).Value = oeuvre.Ville;
                worksheet.Cell(currentRow, 6).Value = $"{oeuvre.Location.Position.Longitude.ToString().Replace(",", ".")},{oeuvre.Location.Position.Latitude.ToString().Replace(",",".")}";
                worksheet.Cell(currentRow, 7).Value = oeuvre.TypeOeuvre;
                worksheet.Cell(currentRow, 8).Value = oeuvre.Informations.Trim();
                worksheet.Cell(currentRow, 9).Value = oeuvre.DatePhotoModification ?? oeuvre.DateCreation;
            }

            var stream = new MemoryStream();
            workbook.SaveAs(stream);
            stream.Seek(0, SeekOrigin.Begin);
            
            return stream;
        }

    }
}
