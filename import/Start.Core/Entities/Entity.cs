using Newtonsoft.Json;

namespace Start.Core.Entities
{
    public abstract class Entity
    {
        [JsonProperty(PropertyName = "partitionKey")]
        public string PartitionKey { get; set; }

        public abstract string GetId(); 
    }
}
