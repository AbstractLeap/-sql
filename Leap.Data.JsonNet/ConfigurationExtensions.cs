namespace Leap.Data.JsonNet {
    using Leap.Data.Configuration;

    public static class ConfigurationExtensions {
        public static Configuration UseJsonNetFieldSerialization(this Configuration configuration) {
            configuration.Serializer = new JsonNetFieldSerializer();
            return configuration;
        }
    }
}