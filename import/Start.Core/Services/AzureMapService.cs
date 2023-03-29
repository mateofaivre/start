using Microsoft.Azure.Cosmos.Spatial;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Start.Core.Configurations;
using Start.Core.Entities;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Start.Core.Services
{
    public class AzureMapService
    {
        private readonly AzureMapConfig _azureMapConfig;
        private readonly HttpClient _httpClient;

        public AzureMapService(IOptions<AzureMapConfig> azureMapConfig)
        {
            _azureMapConfig = azureMapConfig.Value;
            _httpClient = new HttpClient
            {
                BaseAddress = new Uri("https://atlas.microsoft.com/")
            };
        }

        public async Task<Point> GetPositionAsync(Adresse address)
        {
            Point location = null;
            var query = address.ToString();
            var format = "json";
            var language = "fr-FR";
            var countrySet = "FR,EN,US,ES";
            var httpResult = await _httpClient.GetAsync($"search/address/{format}?subscription-key={_azureMapConfig.SubscriptionKey}&typeahead=true&api-version=1&query={query}&language={language}&countrySet={countrySet}&view=Auto");
            
            if (httpResult.StatusCode == System.Net.HttpStatusCode.OK)
            {
                var result = await httpResult.Content.ReadAsStringAsync();
                var data = (JObject)JsonConvert.DeserializeObject(result);

                if (data["results"].HasValues)
                {
                    var position = data["results"][0]["position"];
                    var latitude = position["lat"]?.Value<double>();
                    var longitude = position["lon"]?.Value<double>();

                    location = new Point(longitude.GetValueOrDefault(), latitude.GetValueOrDefault());
                }
            }

            return location;
        }

        public async Task<Adresse> GetAddressAsync(Point location)
        {
            Adresse adresse = null;
            var format = "json";
            var query = Helpers.ParserHelper.ConvertToString(location);

            var httpResult = await _httpClient.GetAsync($"search/address/reverse/{format}?subscription-key={_azureMapConfig.SubscriptionKey}&api-version=1.0&query={query}");

            if (httpResult.StatusCode == System.Net.HttpStatusCode.OK)
            {
                var result = await httpResult.Content.ReadAsStringAsync();
                var data = (JObject)JsonConvert.DeserializeObject(result);

                if (data["addresses"].HasValues)
                {
                    var findAdresse = data["addresses"][0]["address"];
                    var streetNumber = findAdresse["streetNumber"]?.Value<string>();
                    var streetName = findAdresse["streetName"]?.Value<string>();
                    var municipality = findAdresse["municipality"]?.Value<string>();
                    var postalCode = findAdresse["postalCode"]?.Value<string>();
                    var country = findAdresse["country"]?.Value<string>();
                    var freeformAddress = findAdresse["freeformAddress"]?.Value<string>();


                    adresse = new Adresse(freeformAddress);
                    adresse.Pays = country;
                    adresse.Rue = streetName;
                    adresse.Ville = municipality;
                }
            }

            return adresse;
        }
    }
}
