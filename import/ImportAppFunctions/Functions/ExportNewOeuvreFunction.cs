using ImportAppFunctions.Helpers;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using Start.Core.Repositories;
using Start.Core.Services;
using System;
using System.Threading.Tasks;

namespace ImportAppFunctions.Functions
{
    public class ExportNewOeuvreFunction
    {
        private readonly ILogger<ExportNewOeuvreFunction> _logger;
        private readonly OeuvreRepository _oeuvreRepository;
        private readonly AzureStorageService _azureStorageService;

        public ExportNewOeuvreFunction(ILoggerFactory loggerFactory,
            OeuvreRepository oeuvreRepository,
            AzureStorageService azureStorageService)
        {
            _logger = loggerFactory.CreateLogger<ExportNewOeuvreFunction>();
            _oeuvreRepository = oeuvreRepository;
            _azureStorageService = azureStorageService;
        }

        [FunctionName("ExportNewOeuvreFunction")]
        public async Task Run([TimerTrigger("0 0 3 * * 6"/*, 
            #if DEBUG
            RunOnStartup= true
            #endif*/
            )] TimerInfo myTimer,
            ExecutionContext context)
        {
            _logger.LogInformation($"ExportNewOeuvreFunction executed at: {DateTime.Now}");

            var exportService = new ExportService(_logger, _oeuvreRepository, _azureStorageService);
            await exportService.ExportNews();
        }
    }
}
