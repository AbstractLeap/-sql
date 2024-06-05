namespace TildeSql.Internal.Caching {
    using Fasterflect;

    using System;

    public static class ConfigurationExtensions {
        public static Configuration.Configuration EnableCaching<TEntity>(
            this Configuration.Configuration configuration,
            TimeSpan absoluteExpirationRelativeToNow
        ) {
            return configuration;
        }
        public static Configuration.Configuration EnableCaching(
            this Configuration.Configuration configuration,
            string collectionName,
            TimeSpan absoluteExpirationRelativeToNow,
            ICacheKeyProvider cacheKeyProvider) {
            return configuration;
        }
    }
}