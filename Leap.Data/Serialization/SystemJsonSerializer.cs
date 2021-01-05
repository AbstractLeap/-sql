namespace Leap.Data.Serialization {
    using System;
    using System.Text.Json;

    class SystemJsonSerializer : ISerializer {
        public string Serialize(object obj) {
            return JsonSerializer.Serialize(obj);
        }

        public object Deserialize(Type type, string json) {
            return JsonSerializer.Deserialize(json, type);
        }
    }
}