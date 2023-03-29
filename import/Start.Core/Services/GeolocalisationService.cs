using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Start.Core.Helpers;
using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace Start.Core.Services
{
    /// <summary>
    /// Service de géolocalisation
    /// </summary>
    public class GeolocalisationService : IDisposable
    {
        private HttpClient _httpClient;
        private readonly string _clientId = "ZP8as5FzcRJpHvHW";
        private readonly string _clientSecret = "8950ef9f8f5547d483694af7ce1d4b2c";

        /// <summary>
        /// Obtient un token d'accès à l'api de géolocalisation
        /// </summary>
        /// <returns></returns>
        public async Task<string> GetAccessToken()
        {
            _httpClient = new HttpClient
            {
                BaseAddress = new Uri("https://www.arcgis.com/")
            };

            await Task.Delay(3000); /* pour éviter la surcharge du serveur*/
            var httpResult = await _httpClient.GetAsync($"sharing/oauth2/token?client_id={_clientId}&grant_type=client_credentials&client_secret={_clientSecret}&f=pjson");
            var result = await httpResult.Content.ReadAsStringAsync();

            var data = (JObject)JsonConvert.DeserializeObject(result);

            return data["access_token"].Value<string>();
        }

        /// <summary>
        /// Obtient les coordonnées spatial d'une adresse
        /// </summary>
        /// <param name="address">L'adresse à géolocaliser</param>
        /// <param name="token">Le token d'accès</param>
        /// <returns>Les coordonnées spatiales</returns>
        public async Task<Microsoft.Azure.Cosmos.Spatial.Point> GetGeolocationAsync(string address, string token = null)
        {
            Microsoft.Azure.Cosmos.Spatial.Point location = null;
            try
            {
                if (string.IsNullOrEmpty(token))
                {
                    token = await GetAccessToken();
                }

                _httpClient = new HttpClient
                {
                    BaseAddress = new Uri("https://geocode.arcgis.com/")
                };

                await Task.Delay(3000); /* pour éviter la surcharge du serveur*/
                var httpResult = await _httpClient.GetAsync($"arcgis/rest/services/World/GeocodeServer/findAddressCandidates?token={token}&SingleLine={address}&category=&outFields=*&forStorage=false&f=json");
                var result = await httpResult.Content.ReadAsStringAsync();
                var data = (JObject)JsonConvert.DeserializeObject(result);

                var point = ((JArray)data["candidates"])[0]["location"];
                var longString = point["x"].Value<string>();
                var latString = point["y"].Value<string>();

                location = new Microsoft.Azure.Cosmos.Spatial.Point(ParserHelper.ParseDouble(longString), ParserHelper.ParseDouble(latString));
            }
            catch(Exception e)
            {
                System.Diagnostics.Debug.WriteLine(e.Message);
            }

            return location;
        }

        /// <summary>
        /// Obtient une adresse en fonction de coordonnées spatiales
        /// </summary>
        /// <param name="longitude">La longitute</param>
        /// <param name="latitude">La latitude</param>
        /// <param name="token">Le token d'accès à l'api</param>
        /// <returns>L'adresse correspondant aux coordonnées spatiales</returns>
        public async Task<string> GetAddress(string longitude, string latitude, string token = null)
        {
            if (string.IsNullOrEmpty(token))
            {
                token = await GetAccessToken();
            }

            _httpClient = new HttpClient
            {
                BaseAddress = new Uri("https://geocode.arcgis.com/")
            };

            var request = $"arcgis/rest/services/World/GeocodeServer/reverseGeocode?f=json&langCode=FR&location={latitude}%2C{longitude}";
            System.Diagnostics.Debug.WriteLine("request {0}", request);

            await Task.Delay(3000); /* pour éviter la surcharge du serveur*/
            var httpResult = await _httpClient.GetAsync(request);
            var result = await httpResult.Content.ReadAsStringAsync();
            var data = (JObject)JsonConvert.DeserializeObject(result);
            return data["address"]["LongLabel"].Value<string>();
        }

        public void Dispose()
        {
            if (_httpClient != null)
            {
                _httpClient.Dispose();
            }
        }
    }
}
