using ImportAppFunctions.Helpers;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using Start.Core.Readers;
using Start.Core.Services;
using System;
using System.IO;

namespace ImportAppFunctions.Functions
{
    public static class PartenaireKmlFunction
    {
        private const string _containerName = "partenaire-kml-workitems";

        [FunctionName("PartenaireKmlFunction")]
        public static void Run([BlobTrigger("partenaire-kml-workitems")] Stream kmlItem, ILogger log, ExecutionContext context)
        {
            try
            {
                var config = context.GetConfig();
                string appsettingvalue = config["AzureWebJobsStorage"];

                var partenaireItems = KmlReader.ReadKmlPartenaire(kmlItem);
                var importService = new ImportService(appsettingvalue, log, "partenaire");

                importService.InsertOrMerge(partenaireItems);

                log.LogInformation($"C# Queue trigger function processed: {kmlItem}");
            }
            catch (Exception e)
            {
                log.LogError(e, e.StackTrace);
            }
        }
    }
}
