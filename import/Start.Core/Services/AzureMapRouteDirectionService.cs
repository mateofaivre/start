using GeoJSON.Net.Geometry;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Start.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Start.Core.Services
{
    public class AzureMapRouteDirectionService
    {
        public async Task<string> GetRoute(List<Oeuvre> oeuvres)
        {
            if (oeuvres == null || !oeuvres.Any())
                return string.Empty;

            var client = new HttpClient
            {
                BaseAddress = new Uri("https://atlas.microsoft.com/")
            };

            var query = /*"47.305278,5.054722:47.319444,5.060556";*/
                oeuvres.Aggregate(new StringBuilder(), (s, o) =>
                {
                    if (s.Length > 0)
                        s.Append(":");
                    var position = o.Location;
                    var longitude = position.Position.Longitude.ToString().Replace(",", ".");
                    var latitude = position.Position.Latitude.ToString().Replace(",", ".");

                    s.Append($"{longitude},{latitude}");

                    return s;

                }).ToString();

            string subscriptionKey = "VdMYbbyNpmHp66kZU2Fh0F49wyasCau1UfrttezsszM";
            
            var httpResult = await client.GetAsync($"route/directions/json?api-version=1.0&subscription-key={subscriptionKey}&query={query}&travelMode=pedestrian");
            var result = await httpResult.Content.ReadAsStringAsync();

            var data = (JObject)JsonConvert.DeserializeObject(result);
            var sousParcours = (JArray)data["routes"][0]["legs"];

            var points = data["routes"][0]["legs"].SelectMany(p => p["points"]);
            var summary = data["routes"][0]["summary"];
            var distance = summary["lengthInMeters"].Value<int>();
            var temps = summary["travelTimeInSeconds"].Value<int>();

            var geoPositionCollection = new List<Position>();
            foreach(var pt in points)
            {
                var geoPosition = new Position(pt["latitude"].Value<string>(), pt["longitude"].Value<string>());

                geoPositionCollection.Add(geoPosition);
            }

            //var parcoursDetail = new ParcoursDetail(geoPositionCollection, new List<Point>
            //{
            //    new Point(new Position(47.305278, 5.054722)),
            //    new Point(new Position(47.319444, 5.060556)),
            //}) 
            //{
            //    Titre = "Example de parcours",
            //    DistanceEnMetre = distance,
            //    TempsEnSeconds = temps
            //};

            var parcoursDetail = new ParcoursDetail(geoPositionCollection, oeuvres)
            {
                Titre = "Example de parcours",
                DistanceEnMetre = distance,
                TempsEnSeconds = temps
            };

            return parcoursDetail.ToJson();
        }
    }
}

