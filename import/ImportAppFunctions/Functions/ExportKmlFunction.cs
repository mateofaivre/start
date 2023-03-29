using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Start.Core.Repositories;
using Start.Core.Services;
using System.Threading.Tasks;

namespace ImportAppFunctions.Functions
{
    public class ExportKmlFunction
    {
        private readonly ILogger<ExportKmlFunction> _logger;
        private readonly OeuvreRepository _oeuvreRepository;
        private readonly AzureStorageService _azureStorageService;

        public ExportKmlFunction(ILoggerFactory loggerFactory,
            OeuvreRepository oeuvreRepository,
            AzureStorageService azureStorageService)
        {
            _logger = loggerFactory.CreateLogger<ExportKmlFunction>();
            _oeuvreRepository = oeuvreRepository;
            _azureStorageService = azureStorageService;
        }

        [FunctionName("ExportKmlFunction")]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = null)] HttpRequest req)
        {
            _logger.LogInformation("C# HTTP trigger export function processed a request.");
            
            var exportService = new ExportService(_logger, _oeuvreRepository, _azureStorageService);
            var result = await exportService.Backup();

            return new JsonResult(result);
        }
    }
}
