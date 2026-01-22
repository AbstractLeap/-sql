namespace TildeSql.Internal.Caching {
    using System;

    public class CollectionCacheOptions {
        public ICacheKeyProvider CacheKeyProvider { get; set; }

        public TimeSpan AbsoluteExpirationRelativeToNow { get; set; }

        public bool QueryCachingEnabled { get; set; }
    }
}