using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using Start.Core.Repositories;
using Start.Core.Services;
using System;
using System.Threading.Tasks;

namespace ImportAppFunctions.Functions
{
    public class ScheduleBackupFunction
    {
        private readonly ILogger<ScheduleBackupFunction> _logger;
        private readonly OeuvreRepository _oeuvreRepository;
        private readonly AzureStorageService _azureStorageService;

        public ScheduleBackupFunction(ILoggerFactory loggerFactory,
            OeuvreRepository oeuvreRepository,
            AzureStorageService azureStorageService)
        {
            _logger = loggerFactory.CreateLogger<ScheduleBackupFunction>();
            _oeuvreRepository = oeuvreRepository;
            _azureStorageService = azureStorageService;
        }

        [FunctionName("ScheduleBackupFunction")]
        public async Task Run([TimerTrigger("0 0 3 * * *")]TimerInfo myTimer)
        {
            _logger.LogInformation($"C# ScheduleBackupFunction function executed at: {DateTime.Now}");

            var exportService = new ExportService(_logger, _oeuvreRepository, _azureStorageService);
            await exportService.Backup();
        }
    }
}
