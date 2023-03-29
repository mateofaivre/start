using ImportAppFunctions.Helpers;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using Start.Core.Readers;
using Start.Core.Services;
using System;
using System.IO;

namespace ImportAppFunctions.Functions
{
    public static class KmlFunction
    {
        private const string _containerName = "kml-workitems";

        [FunctionName("KmlFunction")]
        public static void Run([BlobTrigger("kml-file")]Stream kmlItem, ILogger log, ExecutionContext context)
        {
            try
            {
                var config = context.GetConfig();
                string appsettingvalue = config["AzureWebJobsStorage"];
                string databaseId = config["databaseid"] ?? "startDB-Prod";

                var oeuvreItems = KmlReader.Read(kmlItem);
                var importService = new ImportService(appsettingvalue, log, database: databaseId);

                importService.InsertOrMerge(oeuvreItems);
                importService.PushGeolocalisationDemand(oeuvreItems);

                log.LogInformation($"C# Queue trigger function processed: {kmlItem}");
            }
            catch(Exception e)
            {
                log.LogError(e, e.StackTrace);
            }
        }

        
    }
}
