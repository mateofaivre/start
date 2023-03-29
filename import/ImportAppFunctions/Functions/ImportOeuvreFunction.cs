using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using Start.Core.Process;
using Start.Core.Repositories;
using Start.Core.Services;
using System;
using System.IO;
using System.Threading.Tasks;

namespace ImportAppFunctions.Functions
{
    public class ImportOeuvreFunction
    {
        private readonly ILogger<ImportOeuvreFunction> _logger;
        private readonly OeuvreRepository _oeuvreRepository;
        private readonly UtilisateurRepository _utilisateurRepository;
        private readonly AzureStorageService _azureStorageService;
        private readonly AzureMapService _azureMapService;

        public ImportOeuvreFunction(
            ILoggerFactory loggerFactory,
            OeuvreRepository oeuvreRepository,
            UtilisateurRepository utilisateurRepository,
            AzureStorageService azureStorageService,
            AzureMapService azureMapService)
        {
            _logger = loggerFactory.CreateLogger<ImportOeuvreFunction>();
            _oeuvreRepository = oeuvreRepository;
            _utilisateurRepository = utilisateurRepository;
            _azureStorageService = azureStorageService;
            _azureMapService = azureMapService;
        }

        [FunctionName("ImportOeuvreFunction")]
        public async Task Run(
            [BlobTrigger("import/{utilisateurId}/wait/{name}.zip", Connection = "AzureStorageConfig:AzureWebJobsStorage")]Stream myBlob, 
            string name, 
            string utilisateurId)
        {
            _logger.LogInformation($"C# Blob trigger ImportOeuvreFunction Processed blob\n Name:{name} \n Size: {myBlob.Length} Bytes");

            bool importSucceeded = false;
            byte[] binary = null;
            var fileName = $"{name}.zip";

            try
            {
                var fileData = new MemoryStream();
                myBlob.CopyTo(fileData);
                binary = fileData.ToArray();
                fileData.Dispose();

                _azureStorageService.DeleteBlob("import", $"{utilisateurId}/wait/{fileName}");

                if (binary.Length == 0)
                {
                    _logger.LogWarning("Le fichier est vide");
                }
                else
                {

                    var utilisateur = await _utilisateurRepository.GetById(utilisateurId, "item");

                    if (utilisateur != null)
                    {
                        var process = new ImportProcess(_logger, _oeuvreRepository, _azureStorageService, _azureMapService);
                        var response = await process.Execute(new Start.Core.Requests.ImportRequest
                        {
                            DateImport = DateTime.Now,
                            Utilisateur = utilisateur,
                            ZipFile = binary
                        });

                        if (response != null && response.Report != null)
                        {
                            //TODO mettre un flag sur l'image pour savoir que ça vient de l'import et connaître son nom d'origine (import :true, nom-import )

                            var uri = await _azureStorageService.UploadBlob("import", $"{utilisateurId}/report/{DateTime.Now:yyyy-MM-dd-hh-mm-ss}{fileName}", new MemoryStream(response.Report));
                            _logger.LogInformation($"Rapport disponible {uri}");
                            importSucceeded = true;
                        }

                        _logger.LogInformation("Import terminé");
                    } 
                }
            }
            catch(Exception e)
            {
                _logger.LogError("Erreur lors de l'import {0}", e.Message);
            }

            if (!importSucceeded)
            {
                await _azureStorageService.UploadBlob("import", $"{utilisateurId}/report/error-{DateTime.Now:yyyy-MM-dd-hh-mm-ss}{fileName}", new MemoryStream(binary));
            }
        }
    }
}
