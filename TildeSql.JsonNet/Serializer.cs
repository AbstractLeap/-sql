namespace TildeSql.JsonNet
{
    using System;

    using Newtonsoft.Json;

    using TildeSql.Serialization;

    public class Serializer : ISerializer {
        private readonly JsonSerializerSettings jsonSerializerSettings;

        public Serializer(JsonSerializerSettings jsonSerializerSettings)
        {
            this.jsonSerializerSettings = jsonSerializerSettings;
        }

        public void Configure(Action<JsonSerializerSettings> action) {
            action(this.jsonSerializerSettings);
        }

        public string Serialize(object obj) {
            return JsonConvert.SerializeObject(obj, this.jsonSerializerSettings);
        }

        public object Deserialize(Type type, string json) {
            return JsonConvert.DeserializeObject(json, type, this.jsonSerializerSettings);
        }
    }
}