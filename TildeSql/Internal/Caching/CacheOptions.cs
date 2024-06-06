namespace TildeSql.Internal.Caching {
    using System.Collections.Generic;

    public class CacheOptions {
        private readonly Dictionary<string, CollectionCacheOptions> cache = new();
        public bool TryGetCacheOptions(string collectionName, out CollectionCacheOptions options) {
            if (this.cache.TryGetValue(collectionName, out options)) return true;
            return false;
        }

        public void Add(string collectionName, CollectionCacheOptions options) {
            this.cache[collectionName] = options;
        }
    }
}