namespace TildeSql.MemoryCache {
    using Microsoft.Extensions.Caching.Memory;
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Logging.Abstractions;
    using Microsoft.Extensions.Options;

    using TildeSql.Configuration;

    public static class ConfigurationExtensions {
        public static Configuration UseMemoryCache(this Configuration configuration) {
            return configuration.UseMemoryCache(new MemoryCacheOptions(), new NullLoggerFactory());
        }

        public static Configuration UseMemoryCache(this Configuration configuration, IOptions<MemoryCacheOptions> optionsAccessor) {
            return configuration.UseMemoryCache(optionsAccessor, new NullLoggerFactory());
        }

        public static Configuration UseMemoryCache(this Configuration configuration, IOptions<MemoryCacheOptions> optionsAccessor, ILoggerFactory loggerFactory) {
            configuration.MemoryCache = new MemoryCacheAdapter(optionsAccessor, loggerFactory);
            return configuration;
        }
    }
}