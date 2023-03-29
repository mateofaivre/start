using Newtonsoft.Json;
using System;

namespace Start.Core.Entities
{
    public static class PartenairePartitionKeys
    {
        public const string Item = "item";
    }

    /// <summary>
    /// Définit les propriétés d'un partenaire sérialisable en json
    /// </summary>
    public class Partenaire
    {
        [JsonProperty(PropertyName = "id")]
        public string Marqueur { get; set; }

        [JsonProperty(PropertyName = "date")]
        public DateTime Date { get; set; }

        [JsonProperty(PropertyName = "dateModification")]
        public DateTime? DateModification { get; set; }

        [JsonProperty(PropertyName = "nom")]
        public string Nom { get; set; }

        [JsonProperty(PropertyName = "imageUrl")]
        public string ImageUrl { get; set; }

        [JsonProperty(PropertyName = "adresse")]
        public string Adresse { get; set; }

        [JsonProperty(PropertyName = "rue")]
        public string Rue { get; set; }

        [JsonProperty(PropertyName = "ville")]
        public string Ville { get; set; }

        [JsonProperty(PropertyName = "codePostal")]
        public string CodePostal { get; set; }

        [JsonProperty(PropertyName = "typePartenaire")]
        public string TypePartenaire { get; set; }

        [JsonProperty(PropertyName = "informations")]
        public string Informations { get; set; }

        [JsonProperty(PropertyName = "telephone")]
        public string Telephone { get; set; }

        [JsonProperty(PropertyName = "siteWeb")]
        public string SiteWeb { get; set; }

        [JsonProperty(PropertyName = "location")]
        public Microsoft.Azure.Cosmos.Spatial.Point Location { get; set; }

        [JsonProperty(PropertyName = "partitionKey")]
        public string PartitionKey { get; set; }

        /// <summary>
        /// Sérialise l'oeuvre en Json
        /// </summary>
        /// <returns></returns>
        public string ToJson()
        {
            return JsonConvert.SerializeObject(this);
        }
    }
}
