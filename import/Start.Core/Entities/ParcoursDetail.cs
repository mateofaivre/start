using GeoJSON.Net.Feature;
using GeoJSON.Net.Geometry;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace Start.Core.Entities
{
    public class ParcoursDetail
    {
        [JsonProperty(PropertyName = "id")]
        public string Identifiant { get; set; }

        [JsonProperty(PropertyName = "distanceEnMetre")]
        public int DistanceEnMetre { get; set; }

        [JsonProperty(PropertyName = "tempsEnSeconds")]
        public int TempsEnSeconds { get; set; }

        [JsonProperty(PropertyName = "titre")]
        public string Titre { get; set; }

        public Feature Feature { get; }

        public FeatureCollection Data { get; set; }

        public ParcoursDetail(IEnumerable<Position> positions, IEnumerable<Oeuvre> oeuvres)
        {
            Data = new FeatureCollection();
            Feature = new Feature(new LineString(positions));

            Feature.Properties.Add("titre", Titre);
            Feature.Properties.Add("id", Identifiant);
            Feature.Properties.Add("distanceEnMetre", DistanceEnMetre);
            Feature.Properties.Add("tempsEnSeconds", TempsEnSeconds);

            foreach (var o in oeuvres)
            {
                var position = new Position(o.Location.Position.Longitude, o.Location.Position.Latitude);
                var oeuvreFeature = new Feature(new Point(position));
                oeuvreFeature.Properties.Add("imageUrl", o.ImageUrl);

                Data.Features.Add(oeuvreFeature);
            }

            Data.Features.Add(Feature);
        }

        public string ToJson()
        {
            Feature.Properties["titre"] = Titre;
            Feature.Properties["id"] = Identifiant;
            Feature.Properties["distanceEnMetre"] = DistanceEnMetre;
            Feature.Properties["tempsEnSeconds"] = TempsEnSeconds;

            return JsonConvert.SerializeObject(Data);
        }
    }
}
