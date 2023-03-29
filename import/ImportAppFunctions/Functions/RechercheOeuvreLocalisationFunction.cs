using ImportAppFunctions.Helpers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Start.Core.Configurations;
using Start.Core.DataSourceContexts;
using Start.Core.Process;
using Start.Core.Repositories;
using Start.Core.Services;
using System.Threading.Tasks;

namespace ImportAppFunctions.Functions
{
    public class RechercheOeuvreLocalisationFunction
    {
        private readonly ILogger<RechercheOeuvreLocalisationFunction> _logger;
        private readonly OeuvreRepository _oeuvreRepository;
        private readonly AzureStorageService _azureStorageService;

        public RechercheOeuvreLocalisationFunction(
            ILoggerFactory loggerFactory,
            OeuvreRepository oeuvreRepository,
            AzureStorageService azureStorageService)
        {
            _logger = loggerFactory.CreateLogger<RechercheOeuvreLocalisationFunction>();
            _oeuvreRepository = oeuvreRepository;
            _azureStorageService = azureStorageService;
        }

        [FunctionName("RechercheOeuvreLocalisationFunction")]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get")] HttpRequest req)
        {
            _logger.LogInformation("C# HTTP trigger RechercheOeuvreLocalisationFunction processed a request.");

            var location = req.Query["location"];
            var userId = req.Query["_userId"].ToString();
            if (string.IsNullOrEmpty(userId))
            {
                userId = req.Query["userId"].ToString();
            }

            var isAdminValue = req.Query["_isAdmin"].ToString();
            if(string.IsNullOrEmpty(isAdminValue))
            {
                isAdminValue = req.Query["isAdmin"].ToString();
            }

            bool.TryParse(isAdminValue, out bool isAdmin);

            var findProcess = new FindOeuvreProcess(_logger, _oeuvreRepository, _azureStorageService);
            var result = await findProcess.Execute(new Start.Core.Requests.FindOeuvreRequest
            {
                BlobFolderName = "br",
                ContainerName = "photos",
                IsAdmin = isAdmin,
                Location = location,
                UrlTimeToLive = new System.TimeSpan(0, 2, 0),
                UserId = userId
            });

            return new JsonResult(result);
        }
    }
}
