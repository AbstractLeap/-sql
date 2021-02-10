namespace Leap.Data.Internal.Caching {
    using System.Text;

    using Leap.Data.Internal.ColumnValueFactories;
    using Leap.Data.Schema;

    static class CacheKeyProvider {
        public static string GetCacheKey<TEntity, TKey>(Collection collection, TEntity entity) {
            var key = collection.KeyExtractor.Extract<TEntity, TKey>(entity);
            return GetCacheKey<TEntity, TKey>(collection, key);
        }

        public static string GetCacheKey<TEntity, TKey>(Collection collection, TKey key) {
            var cacheKey = new StringBuilder(collection.CollectionName);
            foreach (var keyColumn in collection.KeyColumns) {
                var value = collection.KeyColumnValueExtractor.GetValue<TEntity, TKey>(keyColumn, key);
                cacheKey.Append("|").Append(value);
            }

            return cacheKey.ToString();
        }
    }
}