namespace Leap.Data.JsonNet {
    using Leap.Data.Configuration;

    using Newtonsoft.Json;

    public static class ConfigurationExtensions {
        public static Configuration UseJsonNetFieldSerialization(this Configuration configuration) {
            configuration.Serializer = new Serializer(new JsonSerializerSettings { ContractResolver = new FieldsOnlyContractResolver(), TypeNameHandling = TypeNameHandling.Auto });
            return configuration;
        }
    }
}