namespace TildeSql.Internal.Caching {
    using System;
    using System.Threading;
    using System.Threading.Tasks;

    using Microsoft.Extensions.Caching.Distributed;
    using Microsoft.Extensions.Caching.Memory;

    using TildeSql.Queries;

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
            await this.StoreInCacheAsync(entityQuery.CacheKey, this.rows, entityQuery.AbsoluteExpirationRelativeToNow);
        }

        public async ValueTask VisitKeyQueryAsync<TEntity, TKey>(KeyQuery<TEntity, TKey> keyQuery, CancellationToken cancellationToken = default)
            where TEntity : class {
            await this.StoreInCacheAsync(keyQuery.CacheKey, this.rows, keyQuery.AbsoluteExpirationRelativeToNow);
        }

        public async ValueTask VisitMultipleKeyQueryAsync<TEntity, TKey>(MultipleKeyQuery<TEntity, TKey> multipleKeyQuery, CancellationToken cancellationToken = default)
            where TEntity : class {
            if (!this.cacheOptions.TryGetCacheOptions(multipleKeyQuery.Collection.CollectionName, out var collectionCacheOptions)) {
                return;
            }

            foreach (var row in this.rows) {
                var id = multipleKeyQuery.Collection.KeyFactory.Create(row);
                var cacheKey = collectionCacheOptions.CacheKeyProvider.GetEntityCacheKey<TEntity, TKey>(multipleKeyQuery.Collection, (TKey)id);
                await this.StoreInCacheAsync(cacheKey, [row], collectionCacheOptions.AbsoluteExpirationRelativeToNow);
            }
        }
    }
}