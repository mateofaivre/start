using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Start.Core.Configurations;
using Start.Core.DataSourceContexts;
using Start.Core.Process;
using Start.Core.Repositories;
using Start.Core.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace Start.UnitTest
{
    public class ImportProcessTest
    {
        private readonly ImportProcess _importProcess;
        private readonly UtilisateurRepository _utilisateurRepository;
        public ImportProcessTest(ILoggerFactory loggerFactory)
        {
            IOptions<CosmosDbConfig> cosmosDbConfigOptions = new TestCosmosDbConfigOptions();
            var context = new CosmosDbContext(cosmosDbConfigOptions, loggerFactory.CreateLogger<CosmosDbContext>());
            var repository = new OeuvreRepository(context, loggerFactory.CreateLogger<OeuvreRepository>());
            _utilisateurRepository = new UtilisateurRepository(context, loggerFactory.CreateLogger<UtilisateurRepository>());

            IOptions<AzureStorageConfig> azureStorageConfigOptions = new TestAzureStorageConfigOptions();
            var azureStorageService = new AzureStorageService(azureStorageConfigOptions, loggerFactory.CreateLogger<AzureStorageService>());

            IOptions<AzureMapConfig> azureMapConfigOptions = new TestAzureMapConfigOptions();
            var azureMapService = new AzureMapService(azureMapConfigOptions);

            _importProcess = new ImportProcess(
                loggerFactory.CreateLogger<ImportProcess>(),
                repository,
                azureStorageService,
                azureMapService);
        }

        public async Task TestExecute()
        {
            
            byte[] binary = null;
            var ms = new MemoryStream();
            using (var fileStream = File.Open(@"C:\Data\4-Projets\start\import\Start.UnitTest\14533684.zip", FileMode.Open))
            {
                fileStream.CopyTo(ms);
                ms.Seek(0, SeekOrigin.Begin);
            }

            var user = await _utilisateurRepository.GetById("14533684", "item");

            var response = await _importProcess.Execute(new Core.Requests.ImportRequest
            {
                DateImport = DateTime.Now,
                Utilisateur = new Core.Entities.Utilisateur
                {
                    Email = "delacroix.gilles@club-internet.fr",
                    Id = "15789937",
                    Pseudo = "méphisroth",
                },
                ZipFile = ms.ToArray()
            });
        }

    }
}
