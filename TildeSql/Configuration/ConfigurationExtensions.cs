namespace TildeSql.Configuration {
    using System;

    using Microsoft.Extensions.Caching.Distributed;
    using Microsoft.Extensions.Caching.Memory;

    using TildeSql.Internal.Caching;

    public static class ConfigurationExtensions {
        public static Configuration Configure(this Configuration configuration, Action<Configuration> options) {
            options?.Invoke(configuration);
            return configuration;
        }

        public static Configuration UseMemoryCache(this Configuration configuration, IMemoryCache memoryCache) {
            configuration.MemoryCache = memoryCache ?? throw new ArgumentNullException(nameof(memoryCache));
            return configuration;
        }

        public static Configuration UseDistributedCache(this Configuration configuration, IDistributedCache distributedCache) {
            configuration.DistributedCache = distributedCache ?? throw new ArgumentNullException(nameof(distributedCache));
            return configuration;
        }

        public static Configuration EnableCaching<TEntity>(this Configuration configuration, TimeSpan absoluteExpirationRelativeToNow)
            => configuration.EnableCaching(configuration.Schema.GetDefaultCollection<TEntity>().CollectionName, absoluteExpirationRelativeToNow, new DefaultCacheKeyProvider(), false);

        public static Configuration EnableCaching(
            this Configuration configuration,
            string collectionName,
            TimeSpan absoluteExpirationRelativeToNow,
            ICacheKeyProvider cacheKeyProvider,
            bool queryCachingEnabled) {
            configuration.CacheOptions ??= new CacheOptions();
            configuration.CacheOptions.Add(collectionName, new CollectionCacheOptions {
                AbsoluteExpirationRelativeToNow = absoluteExpirationRelativeToNow,
                QueryCachingEnabled = queryCachingEnabled,
                CacheKeyProvider = cacheKeyProvider
            });
            return configuration;
        }
    }
}