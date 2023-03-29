using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Start.Core.Entities;
using Start.Core.Services;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ImportAppFunctions.Functions
{
    public static class RechercheParcoursFunction
    {
        [FunctionName("RechercheParcoursFunction")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("C# HTTP trigger 'RechercheParcoursFunction' processed a request.");

            var oeuvresIds = req.Query["ids"].ToString().Split(',');

            var oeuvres = new List<Oeuvre>();
            var oeuvreService = new CosmosDbService(log);

            foreach (var id in oeuvresIds)
            {
                oeuvres.Add(await oeuvreService.FindOeuvre(id));
            }

            var service = new AzureMapRouteDirectionService();
            var data = await service.GetRoute(oeuvres);
            return new ContentResult { Content = data, ContentType = "application/json" };
        }
    }
}

