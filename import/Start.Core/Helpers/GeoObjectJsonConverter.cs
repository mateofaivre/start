using Azure.Core.GeoJson;
using Newtonsoft.Json;
using System;
using System.Diagnostics.CodeAnalysis;

namespace Start.Core.Helpers
{
    public class GeoObjectJsonConverter : JsonConverter<GeoObject>
    {
        public override GeoObject ReadJson(JsonReader reader, Type objectType, [AllowNull] GeoObject existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }

        public override void WriteJson(JsonWriter writer, [AllowNull] GeoObject value, JsonSerializer serializer)
        {
            writer.WriteRaw(value.ToString());
            //JToken t = JToken.FromObject(value);
            

            //if (t.Type != JTokenType.Object)
            //{
            //    t.WriteTo(writer);
            //}
            //else
            //{
            //    JObject o = (JObject)t;
            //    IList<string> propertyNames = o.Properties().Select(p => p.Name).ToList();

            //    o.AddFirst(new JProperty("Keys", new JArray(propertyNames)));

            //    o.WriteTo(writer);
            //}
        }
    }
}
