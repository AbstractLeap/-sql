namespace TildeSql.Configuration {
    using System;

    using Microsoft.Extensions.Caching.Memory;

    public static class ConfigurationExtensions {
        public static Configuration Configure(this Configuration configuration, Action<Configuration> options) {
            options?.Invoke(configuration);
            return configuration;
        }

        public static Configuration UseMemoryCache(this Configuration configuration, IMemoryCache memoryCache) {
            configuration.MemoryCache = memoryCache ?? throw new ArgumentNullException(nameof(memoryCache));
            return configuration;
        }
    }
}