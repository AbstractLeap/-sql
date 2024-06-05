namespace TildeSql.Internal.Caching {
    using System;

    internal class CollectionCacheOptions {
        public ICacheKeyProvider CacheKeyProvider { get; set; }

        public TimeSpan? AbsoluteExpirationRelativeToNow { get; set; }

        public bool QueryCachingEnabled { get; set; }
    }
}