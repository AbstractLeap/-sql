namespace TildeSql.Internal.Caching {
    using System;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;

    using Microsoft.Extensions.Caching.Distributed;
    using Microsoft.Extensions.Caching.Memory;

    using TildeSql.Queries;
    using TildeSql.Schema;

    class CacheSetter : IAsyncQueryVisitor {
        private readonly IMemoryCache memoryCache;

        private readonly IDistributedCache distributedCache;

        private readonly ICacheSerializer cacheSerializer;

        private readonly CacheOptions cacheOptions;

        private object[][] rows;

        public CacheSetter(IMemoryCache memoryCache, IDistributedCache distributedCache,
                           ICacheSerializer cacheSerializer,
                           CacheOptions cacheOptions) {
            this.memoryCache      = memoryCache;
            this.distributedCache = distributedCache;
            this.cacheSerializer  = cacheSerializer;
            this.cacheOptions     = cacheOptions;
        }

        public async ValueTask RemoveAsync<TEntity, TKey>(TEntity entity, Collection collection) {
            if (!this.cacheOptions.TryGetCacheOptions(collection.CollectionName, out var cacheCollectionOptions)) {
                return;
            }

            var key = collection.GetKey<TEntity, TKey>(entity);
            var cacheKey = cacheCollectionOptions.CacheKeyProvider.GetEntityCacheKey<TEntity, TKey>(collection, key);
            if (cacheKey != null) {
                this.memoryCache?.Remove(cacheKey);
                if (this.distributedCache != null) {
                    await this.distributedCache.RemoveAsync(cacheKey);
                }
            }
        }

        public async ValueTask SetAsync<TEntity, TKey>(TEntity entity, Collection collection, DatabaseRow databaseRow) {
            if (!this.cacheOptions.TryGetCacheOptions(collection.CollectionName, out var cacheCollectionOptions)) {
                return;
            }

            var key = collection.GetKey<TEntity, TKey>(entity);
            var cacheKey = cacheCollectionOptions.CacheKeyProvider.GetEntityCacheKey<TEntity, TKey>(collection, key);
            if (cacheKey != null) {
                await StoreInCacheAsync(cacheKey, [databaseRow.Values], cacheCollectionOptions.AbsoluteExpirationRelativeToNow);
            }
        }

        public async ValueTask SetAsync(IQuery query, object[][] rows) {
            this.rows = rows;
            await query.AcceptAsync(this);
        }

        async ValueTask StoreInCacheAsync(string cacheKey, object[][] rows, TimeSpan? absoluteExpirationRelativeToNow) {
            if (this.memoryCache != null) {
                if (absoluteExpirationRelativeToNow.HasValue) {
                    this.memoryCache.Set(cacheKey, rows, absoluteExpirationRelativeToNow.Value);
                }
                else {
                    this.memoryCache.Set(cacheKey, rows);
                }
            }

            if (this.distributedCache != null) {
                var buffer = this.cacheSerializer.Serialize(rows);
                await this.distributedCache.SetAsync(cacheKey, buffer, new DistributedCacheEntryOptions {
                    AbsoluteExpirationRelativeToNow = absoluteExpirationRelativeToNow
                });
            }
        }

        public async ValueTask VisitEntityQueryAsync<TEntity>(EntityQuery<TEntity> entityQuery, CancellationToken cancellationToken = default)
            where TEntity : class {
            var resolvedCacheOptions = entityQuery.ResolvedCacheOptions().SingleOrDefault();
            if (resolvedCacheOptions != default) {
                await this.StoreInCacheAsync(resolvedCacheOptions.cacheKey, this.rows, resolvedCacheOptions.absoluteExpirationRelativeToNow);
            }            
        }

        public async ValueTask VisitKeyQueryAsync<TEntity, TKey>(KeyQuery<TEntity, TKey> keyQuery, CancellationToken cancellationToken = default)
            where TEntity : class {
            var resolvedCacheOptions = keyQuery.ResolvedCacheOptions().SingleOrDefault();
            if (resolvedCacheOptions != default) {
                await this.StoreInCacheAsync(resolvedCacheOptions.cacheKey, this.rows, resolvedCacheOptions.absoluteExpirationRelativeToNow);
            }
        }

        public async ValueTask VisitMultipleKeyQueryAsync<TEntity, TKey>(MultipleKeyQuery<TEntity, TKey> multipleKeyQuery, CancellationToken cancellationToken = default)
            where TEntity : class {
            if (!this.cacheOptions.TryGetCacheOptions(multipleKeyQuery.Collection.CollectionName, out var collectionCacheOptions)) {
                return;
            }

            if (!multipleKeyQuery.ResolvedCacheOptions().Any()) {
                return;
            }

            // bit of a cheat here ... we don't use the resolved cache options as we know they can only be generated from the collectionCacheOptions (i.e. it's not possible to set explicit keys and caches on Multiple queries)
            foreach (var row in this.rows) {
                var id = multipleKeyQuery.Collection.KeyFactory.Create(row);
                var cacheKey = collectionCacheOptions.CacheKeyProvider.GetEntityCacheKey<TEntity, TKey>(multipleKeyQuery.Collection, (TKey)id);
                await this.StoreInCacheAsync(cacheKey, [row], collectionCacheOptions.AbsoluteExpirationRelativeToNow);
            }
        }
    }
}