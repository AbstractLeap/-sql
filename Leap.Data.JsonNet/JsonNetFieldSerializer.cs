namespace Leap.Data.JsonNet {
    using Newtonsoft.Json;

    public class JsonNetFieldSerializer : Serializer {
        public JsonNetFieldSerializer()
            : base(new JsonSerializerSettings { ContractResolver = new FieldsOnlyContractResolver(), TypeNameHandling = TypeNameHandling.Auto }) { }
    }
}