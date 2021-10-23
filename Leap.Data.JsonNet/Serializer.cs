namespace Leap.Data.JsonNet
{
    using System;

    using Leap.Data.Serialization;

    using Newtonsoft.Json;

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