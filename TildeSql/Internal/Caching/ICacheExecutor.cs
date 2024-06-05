namespace TildeSql.Internal.Caching {
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;

    using Microsoft.Extensions.Caching.Memory;
    using Microsoft.Extensions.Caching.Distributed;

    using TildeSql.Queries;

    public interface ICacheSerializer {
        /// <summary>
        ///     Serialized the specified <paramref name="obj" /> into a byte[].
        /// </summary>
        /// <typeparam name="T">The type of the <paramref name="obj" /> parameter.</typeparam>
        /// <param name="obj"></param>
        /// <returns>The byte[] which represents the serialized <paramref name="obj" />.</returns>
        byte[] Serialize<T>(T obj);

        /// <summary>
        ///     Deserialized the specified byte[] <paramref name="data" /> into an object of type <typeparamref name="T" />.
        /// </summary>
        /// <typeparam name="T">The type of the object to be returned.</typeparam>
        /// <param name="data"></param>
        /// <returns>The deserialized object.</returns>
        T Deserialize<T>(byte[] data);
    }

    internal interface ICacheExecutor {
        ValueTask<ExecuteResult> ExecuteAsync(IList<IQuery> queries, CancellationToken cancellationToken = default);

        IAsyncEnumerable<object[]> GetAsync<TEntity>(IQuery query)
            where TEntity : class;
    }

    class CacheExecutor : ICacheExecutor, IAsyncQueryVisitor {
        private readonly IMemoryCache memoryCache;

        private readonly IDistributedCache distributedCache;

        private readonly ICacheSerializer cacheSerializer;

        private readonly CacheOptions cacheOptions;

        private readonly ResultCache resultCache;

        private readonly HashSet<IQuery> executedQueries = new();

        private readonly Dictionary<IQuery, (IQuery Executed, IQuery Remaining)> partiallyExecutedQueries = new();

        public CacheExecutor(IMemoryCache memoryCache, IDistributedCache distributedCache, ICacheSerializer cacheSerializer, CacheOptions cacheOptions) {
            this.memoryCache      = memoryCache;
            this.distributedCache = distributedCache;
            this.cacheSerializer  = cacheSerializer;
            if (this.distributedCache != null && cacheSerializer == null) {
                throw new ArgumentException("If you're using a distributed cache you must also specify the cache serializer");
            }

            this.cacheOptions     = cacheOptions ?? throw new ArgumentNullException(nameof(cacheOptions));
            this.resultCache      = new ResultCache();
        }

        public async ValueTask SetAsync(IQuery query, object[][] rows) {
            if (this.memoryCache != null) {
                if (query.AbsoluteExpirationRelativeToNow.HasValue) {
                    this.memoryCache.Set(query.CacheKey, rows, query.AbsoluteExpirationRelativeToNow.Value);
                } else {
                    this.memoryCache.Set(query.CacheKey, rows);
                }
            }

            if (this.distributedCache != null) {
                var buffer = this.cacheSerializer.Serialize(rows);
                await this.distributedCache.SetAsync(query.CacheKey, buffer, new DistributedCacheEntryOptions {
                    AbsoluteExpirationRelativeToNow = query.AbsoluteExpirationRelativeToNow
                });
            }
        }

        public ValueTask<ExecuteResult> ExecuteAsync(IList<IQuery> queries, CancellationToken cancellationToken = default) {
            throw new NotImplementedException();
        }

        public IAsyncEnumerable<object[]> GetAsync<TEntity>(IQuery query)
            where TEntity : class {
            throw new NotImplementedException();
        }

        public ValueTask VisitEntityQueryAsync<TEntity>(EntityQuery<TEntity> entityQuery, CancellationToken cancellationToken = default)
            where TEntity : class {
            throw new NotImplementedException();
        }

        public async ValueTask VisitKeyQueryAsync<TEntity, TKey>(KeyQuery<TEntity, TKey> keyQuery, CancellationToken cancellationToken = default)
            where TEntity : class {
            if (!this.cacheOptions.TryGetCacheOptions(keyQuery.Collection.CollectionName, out var collectionCacheOptions)) {
                return;
            }

            var key = collectionCacheOptions.CacheKeyProvider.GetEntityCacheKey<TEntity, TKey>(keyQuery.Collection, keyQuery.Key);
            if (string.IsNullOrWhiteSpace(key)) {
                return;
            }

            keyQuery.CacheQuery = true;
            keyQuery.AbsoluteExpirationRelativeToNow = collectionCacheOptions.AbsoluteExpirationRelativeToNow;
            keyQuery.CacheKey = key;

            if (this.memoryCache != null) {
                if (this.memoryCache.TryGetValue(key, out object[][] rows)) {
                    this.resultCache.Add(keyQuery, rows);
                    this.executedQueries.Add(keyQuery);
                    return;
                }
            }

            if (this.distributedCache != null) {
                var cacheBuffer = await this.distributedCache.GetAsync(key, cancellationToken);
                if (cacheBuffer != null) {
                    var cacheRow = this.cacheSerializer.Deserialize<object[]>(cacheBuffer);
                    if (cacheRow != null) {
                        if (this.memoryCache != null) {
                            if (collectionCacheOptions.AbsoluteExpirationRelativeToNow.HasValue) {
                                this.memoryCache.Set(key, cacheRow, collectionCacheOptions.AbsoluteExpirationRelativeToNow.Value);
                            }
                            else {
                                this.memoryCache.Set(key, cacheRow);
                            }
                        }

                        this.resultCache.Add(keyQuery, new List<object[]> { cacheRow });
                        this.executedQueries.Add(keyQuery);
                    }
                }
            }
        }

        public ValueTask VisitMultipleKeyQueryAsync<TEntity, TKey>(MultipleKeyQuery<TEntity, TKey> multipleKeyQuery, CancellationToken cancellationToken = default)
            where TEntity : class {
            throw new NotImplementedException();
        }
    }
}