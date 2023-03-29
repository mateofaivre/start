using Newtonsoft.Json;

namespace Start.Core.Entities
{
    public class Utilisateur : Entity
    {
        [JsonProperty(PropertyName = "id")]
        public string Id { get; set; }

        [JsonProperty(PropertyName = "email")]
        public string Email { get; set; }

        [JsonProperty(PropertyName = "pseudo")]
        public string Pseudo { get; set; }

        public override string GetId()
        {
            return Id;
        }
    }
}
