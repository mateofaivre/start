using Microsoft.Extensions.Logging;
using Start.Core.DataSourceContexts;
using Start.Core.Entities;
using Start.Core.Process;
using Start.Core.Repositories;
using Start.Core.Services;
using System;
using System.Threading.Tasks;

namespace Start.UnitTest
{
    public class FindOeuvreProcessTest
    {
        private readonly FindOeuvreProcess _process;
        private readonly AzureStorageService _azureStorageService;
        private readonly CosmosDbContext _cosmosDbContext;
        private readonly OeuvreRepository _oeuvreRepository;

        public FindOeuvreProcessTest(ILoggerFactory loggerFactory)
        {
            _cosmosDbContext = new CosmosDbContext(new TestCosmosDbConfigOptions(), loggerFactory.CreateLogger<CosmosDbContext>());
            _oeuvreRepository = new OeuvreRepository(_cosmosDbContext, loggerFactory.CreateLogger<OeuvreRepository>());
            _azureStorageService = new AzureStorageService(new TestAzureStorageConfigOptions(), loggerFactory.CreateLogger<AzureStorageService>());
            _process = new FindOeuvreProcess(loggerFactory.CreateLogger<FindOeuvreProcess>(), _oeuvreRepository, _azureStorageService);
        }

        public async Task<SearchGlobalQueryResult> TestGetWithLocation()
        {
            var location = "{\"type\":\"Polygon\",\"coordinates\":[[[46.555083,-1.104126],[48.814099,-1.104126],[48.814099,0.955811],[46.555083,0.955811],[46.555083,-1.104126]]]}";
            return await _process.Execute(
                new Core.Requests.FindOeuvreRequest
                {
                    BlobFolderName = "hd",
                    ContainerName = "photos",
                    IsAdmin = true,
                    UserId = "start",
                    Location = location,
                    UrlTimeToLive = new TimeSpan(0, 2, 0)
                });
        }

        public async Task<Uri> TestGetBlob()
        {
            var blobId = "25d3f85e-3e01-4979-a4e8-7d7dace85165";
            var oeuvre = new Oeuvre
            {
                DateCreation = new DateTime(2022, 8, 1),
                Marqueur = blobId,
            };

            var blobName = "hd/" + oeuvre.DateCreation.ToString("yyyyMM") + "/" + oeuvre.Marqueur + ".jpg";
            var containerName = "photos";
            return await _azureStorageService.GetTemporaryBlobUrl(blobName, containerName, new TimeSpan(0, 2, 0));
        }
    }
}
