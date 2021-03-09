namespace Leap.Data.Configuration {
    using System;

    public static class ConfigurationExtensions {
        public static Configuration Configure(this Configuration configuration, Action<Configuration> options) {
            options?.Invoke(configuration);
            return configuration;
        }
    }
}