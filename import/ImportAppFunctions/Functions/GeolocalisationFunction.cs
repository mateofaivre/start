using ImportAppFunctions.Helpers;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Start.Core.Entities;
using Start.Core.Services;
using System;
using System.Threading.Tasks;

namespace ImportAppFunctions.Functions
{
    public static class GeolocalisationFunction
    {
        [FunctionName("GeolocalisationFunction")]
        public static async Task Run([QueueTrigger("geolocalisation-items")]string oeuvreItem, ILogger log, ExecutionContext context)
        {
            log.LogInformation($"C# Queue trigger function processed: {oeuvreItem}");

            try
            {
                var config = context.GetConfig();
                string appsettingvalue = config["AzureWebJobsStorage"];

                var importService = new ImportService(appsettingvalue, log);
                var oeuvre = JsonConvert.DeserializeObject<Oeuvre>(oeuvreItem);

                if (oeuvre.Location == null && !string.IsNullOrEmpty(oeuvre.ImageUrl))
                {
                    var imageExifService = new ImageExifService();
                    oeuvre.Location = await imageExifService.GetOeuvreGeolocalisation(oeuvre);
                }

                using (var geolocalisationService = new GeolocalisationService())
                {
                    if (oeuvre.Location != null && string.IsNullOrEmpty(oeuvre.Adresse))
                    {
                        var position = oeuvre.Location;
                        var longitude = position.Position.Longitude.ToString().Replace(",", ".");
                        var latitude = position.Position.Latitude.ToString().Replace(",", ".");

                        oeuvre.Adresse = await geolocalisationService.GetAddress(longitude, latitude);
                    }
                    else if (!string.IsNullOrEmpty(oeuvre.Adresse) && oeuvre.Location == null)
                    {
                        oeuvre.Location = await geolocalisationService.GetGeolocationAsync(oeuvre.Adresse);
                    }
                }
                
                importService.InsertOrMerge(new[] {oeuvre});
            }
            catch (Exception ex)
            {
                log.LogError(ex, ex.StackTrace);
            }

        }
    }
}
