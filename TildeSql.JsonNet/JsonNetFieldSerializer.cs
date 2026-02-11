namespace TildeSql.JsonNet {
    using System.Collections.Generic;

    using Newtonsoft.Json;

    public class JsonNetFieldSerializer : Serializer {
        public JsonNetFieldSerializer()
            : base(GetSettings()) { }

        public static JsonSerializerSettings GetSettings() => new JsonSerializerSettings {
            ContractResolver = new FieldsOnlyContractResolver(),
            TypeNameHandling = TypeNameHandling.Auto,
            Converters = new List<JsonConverter> { new ComplexKeyDictionaryConverter() }
        };
    }
}