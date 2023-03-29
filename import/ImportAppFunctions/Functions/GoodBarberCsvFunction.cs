using ImportAppFunctions.Helpers;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using Start.Core.Readers;
using Start.Core.Services;
using System;
using System.IO;

namespace ImportAppFunctions.Functions
{
    public static class GoodBarberCsvFunction
    {
        private const string _containerName = "goodbarber-csv-queue-items";

        [FunctionName("GoodBarberCsvFunction")]
        public static void Run([BlobTrigger(_containerName)]Stream csvItem, ILogger log, ExecutionContext context)
        {
            try
            {
                log.LogInformation($"C# Queue trigger function processed: {csvItem}");
                var config = context.GetConfig();
                string appsettingvalue = config["AzureWebJobsStorage"];
                var oeuvreItems = GoodBarberReader.ReadCsvOeuvre(csvItem);
                var importService = new ImportService(appsettingvalue, log);

                importService.InsertOrMerge(oeuvreItems);
                importService.PushGeolocalisationDemand(oeuvreItems);
            }
            catch(Exception ex)
            {
                log.LogError(ex, ex.StackTrace);
            }
        }
    }
}
