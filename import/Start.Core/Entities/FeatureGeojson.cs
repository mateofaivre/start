using Azure.Core.GeoJson;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace Start.Core.Entities
{
    public class FeatureGeojson
    {
        //[JsonProperty(PropertyName = "geometry")]
        //public GeoObject Geometry { get; set; }

        private GeoObject _geometry;

        [JsonProperty(PropertyName = "geometry")]
        public string Geometry { get; set; }

        [JsonProperty(PropertyName = "properties")]
        public IPropertiesGeojson Properties { get; set; }

        [JsonProperty(PropertyName = "type")]
        public string Type { get { return "Feature"; } }

        public FeatureGeojson(GeoObject geometry, IPropertiesGeojson properties)
        {
            _geometry = geometry;
            Geometry = geometry.ToString();
            Properties = properties;
        }
    }
}
