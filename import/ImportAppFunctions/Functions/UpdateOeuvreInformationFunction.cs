using Azure.Maps.Search.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Cosmos.Spatial;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Start.Core.Entities;
using Start.Core.Process;
using Start.Core.Repositories;
using Start.Core.Requests;
using Start.Core.Services;
using System.IO;
using System.Threading.Tasks;

namespace ImportAppFunctions.Functions
{
    public class UpdateOeuvreInformationFunction
    {
        private readonly ILogger<UpdateOeuvreInformationFunction> _logger;
        private readonly OeuvreRepository _oeuvreRepository;
        private readonly AzureStorageService _azureStorageService;

        public UpdateOeuvreInformationFunction(
            ILoggerFactory loggerFactory,
            OeuvreRepository oeuvreRepository,
            AzureStorageService azureStorageService)
        {
            _logger = loggerFactory.CreateLogger<UpdateOeuvreInformationFunction>();
            _oeuvreRepository = oeuvreRepository;
            _azureStorageService = azureStorageService;
        }

        [FunctionName("UpdateOeuvreInformationFunction")]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = null)] HttpRequest req)
        {
            _logger.LogInformation("C# HTTP trigger UpdateOeuvreInformationFunction processed a request.");
            
            var formData = await req.ReadFormAsync();

            //Get location
            var position = formData["position"].ToString();
            Point location = null;
            if (!string.IsNullOrEmpty(position))
            {
                var coordinate = JsonConvert.DeserializeObject<Location>(position);
                location = new Point(new Position(coordinate.Latitude, coordinate.Longitude));
            }
            
            bool.TryParse(formData["isAdmin"].ToString(), out bool isAdmin);

            var serSettings = new JsonSerializerSettings
            {
                ContractResolver = new CamelCasePropertyNamesContractResolver()
            };

            var addressStringValue = formData["adresse"].ToString();
            var address = JsonConvert.DeserializeObject<AzureMapAddress>(addressStringValue);

            var binary = formData["rawBinary"];
            var updateOeuvreRequest = new UpdateOeuvreRequest
            {
                Adresse = address.FreeformAddress,
                Artiste = formData["artist"].ToString(),
                IdOeuvre = formData["idOeuvre"].ToString(),
                Informations = formData["information"].ToString(),
                IsAdmin = isAdmin,
                Location = location,
                RawData = formData["rawData"].ToString(),
                //RawBinary = formData["rawBinary"].ToArray,
                Rue = string.IsNullOrEmpty(address.StreetNameAndNumber)
                ? address.StreetName
                : address.StreetNameAndNumber,
                TypeOeuvre = formData["typeOeuvre"].ToString(),
                UserId = formData["userId"].ToString(),
                UserMail = formData["userMail"].ToString(),
                UserPseudo = formData["userPseudo"].ToString(),
                Ville = address.Municipality,
                Pays = address.Country
            };

            _logger.LogInformation($"UpdateOeuvreInformationFunction rawData {updateOeuvreRequest.RawData}");

            var updateOeuvreProcess = new UpdateOeuvreProcess(_logger, _oeuvreRepository, _azureStorageService);
            var newOeuvre = await updateOeuvreProcess.Excecute(updateOeuvreRequest);

            if(newOeuvre != null)
            {
                return new OkObjectResult(new OeuvreGeoJson(newOeuvre));
            }
            else
            {
                return new BadRequestObjectResult("Error during uploading oeuvre");
            }
        }
    }
}

