namespace Leap.Data.Internal.Caching {
    using System.Text;

    using Leap.Data.Internal.ColumnValueFactories;
    using Leap.Data.Schema;

    static class CacheKeyProvider {
        public static string GetCacheKey<TEntity, TKey>(Table table, TEntity entity) {
            var key = table.KeyExtractor.Extract<TEntity, TKey>(entity);
            return GetCacheKey<TEntity, TKey>(table, key);
        }

        public static string GetCacheKey<TEntity, TKey>(Table table, TKey key) {
            var cacheKey = new StringBuilder(table.CollectionName);
            foreach (var keyColumn in table.KeyColumns) {
                var value = table.KeyColumnValueExtractor.GetValue<TEntity, TKey>(keyColumn, key);
                cacheKey.Append("|").Append(value);
            }

            return cacheKey.ToString();
        }
    }
}