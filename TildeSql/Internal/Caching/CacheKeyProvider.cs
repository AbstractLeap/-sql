namespace TildeSql.Internal.Caching {
    using System.Text;

    using TildeSql.Schema;

    static class CacheKeyProvider {
        public static string GetCacheKey<TEntity, TKey>(Collection collection, TEntity entity) {
            var key = collection.GetKey<TEntity, TKey>(entity);
            return GetCacheKey<TEntity, TKey>(collection, key);
        }

        public static string GetCacheKey<TEntity, TKey>(Collection collection, TKey key) {
            var cacheKey = new StringBuilder(collection.CollectionName);
            foreach (var keyColumn in collection.KeyColumns) {
                var value = collection.GetKeyColumnValue<TEntity, TKey>(key, keyColumn);
                cacheKey.Append("|").Append(value);
            }

            return cacheKey.ToString();
        }
    }
}