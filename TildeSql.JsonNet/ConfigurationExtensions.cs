﻿namespace TildeSql.JsonNet {
    using TildeSql.Configuration;

    public static class ConfigurationExtensions {
        public static Configuration UseJsonNetFieldSerialization(this Configuration configuration) {
            configuration.Serializer = new JsonNetFieldSerializer();
            return configuration;
        }
    }
}