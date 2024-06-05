namespace TildeSql.Internal.Caching {
    using TildeSql.Queries;
    using TildeSql.Schema;

    public interface ICacheKeyProvider {
        string GetEntityCacheKey<TEntity, TKey>(Collection collection, TKey key);
        string GetEntityQueryCacheKey<TEntity, TKey>(Collection collection, EntityQuery<TEntity> entityQuery) where TEntity : class;
    }
}