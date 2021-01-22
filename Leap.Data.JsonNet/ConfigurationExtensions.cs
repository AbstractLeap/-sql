namespace Leap.Data.JsonNet
{
    using System;

    using Leap.Data.Configuration;
    using Leap.Data.Internal;

    using Newtonsoft.Json;

    public static class ConfigurationExtensions {
        public static Configuration UseJsonNetFieldSerialization(this Configuration configuration) {
            configuration.Serializer = new Serializer(new JsonSerializerSettings() {
                    ContractResolver = new FieldsOnlyContractResolver()
            });
            return configuration;
        }
    }
}