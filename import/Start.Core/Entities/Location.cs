using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace Start.Core.Entities
{
    public class Location
    {
        [JsonProperty(PropertyName = "lon")]
        public double Longitude { get; set; }

        [JsonProperty(PropertyName = "lat")]
        public double Latitude { get; set; }
    }
}
