namespace TildeSql.Internal.Caching {
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;

    using Microsoft.Extensions.Caching.Distributed;
    using Microsoft.Extensions.Caching.Memory;

    using TildeSql.Queries;

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

            this.cacheOptions = cacheOptions ?? throw new ArgumentNullException(nameof(cacheOptions));
            this.resultCache  = new ResultCache();
        }

        public async ValueTask<ExecuteResult> ExecuteAsync(IList<IQuery> queries, CancellationToken cancellationToken = default) {
            this.executedQueries.Clear();
            this.partiallyExecutedQueries.Clear();

            foreach (var query in queries) {
                await query.AcceptAsync(this, cancellationToken);
            }

            var executed = queries.Where(q => this.executedQueries.Contains(q));
            var partiallyExecuted = this.partiallyExecutedQueries.Where(q => queries.Contains(q.Key)).Select(q => (q.Key, q.Value.Executed, q.Value.Remaining));
            var nonExecutedQueries = queries.Where(q => !this.executedQueries.Contains(q) && !this.partiallyExecutedQueries.ContainsKey(q));
            return new ExecuteResult(executed, partiallyExecuted, nonExecutedQueries);
        }

        public IAsyncEnumerable<object[]> GetAsync<TEntity>(IQuery query)
            where TEntity : class {
            return this.Get(query).ToAsyncEnumerable();
        }

        private IEnumerable<object[]> Get(IQuery query) {
            if (this.resultCache.TryGetValue<object[]>(query, out var result)) {
                foreach (var row in result) {
                    yield return row;
                }
            }
            else {
                throw new Exception($"{nameof(CacheExecutor)} did not execute {query} so can not get result");
            }
        }

        public async ValueTask VisitEntityQueryAsync<TEntity>(EntityQuery<TEntity> entityQuery, CancellationToken cancellationToken = default)
            where TEntity : class {
            CollectionCacheOptions collectionCacheOptions = null;
            if (entityQuery.CacheQuery is false) { // caching disabled via query
                return;
            }

            if (entityQuery.CacheKey == null || !entityQuery.AbsoluteExpirationRelativeToNow.HasValue) {
                if (!this.cacheOptions.TryGetCacheOptions(entityQuery.Collection.CollectionName, out collectionCacheOptions)) {
                    return;
                }
            }

            var cacheKey = entityQuery.CacheKey ?? collectionCacheOptions.CacheKeyProvider.GetEntityQueryCacheKey<TEntity>(entityQuery.Collection, entityQuery);
            if (string.IsNullOrWhiteSpace(cacheKey)) {
                return;
            }

            await this.TryFindInCacheAsync(entityQuery, cancellationToken, entityQuery.AbsoluteExpirationRelativeToNow ?? collectionCacheOptions?.AbsoluteExpirationRelativeToNow, cacheKey);
        }

        public async ValueTask VisitKeyQueryAsync<TEntity, TKey>(KeyQuery<TEntity, TKey> keyQuery, CancellationToken cancellationToken = default)
            where TEntity : class {
            if (!this.cacheOptions.TryGetCacheOptions(keyQuery.Collection.CollectionName, out var collectionCacheOptions)) {
                return;
            }

            var cacheKey = collectionCacheOptions.CacheKeyProvider.GetEntityCacheKey<TEntity, TKey>(keyQuery.Collection, keyQuery.Key);
            if (string.IsNullOrWhiteSpace(cacheKey)) {
                return;
            }

            await this.TryFindInCacheAsync(keyQuery, cancellationToken, keyQuery.AbsoluteExpirationRelativeToNow ?? collectionCacheOptions?.AbsoluteExpirationRelativeToNow, cacheKey);
        }

        private async Task TryFindInCacheAsync<TEntity>(QueryBase<TEntity> keyQuery, CancellationToken cancellationToken, TimeSpan? absoluteExpirationRelativeToNow, string cacheKey)
            where TEntity : class {
            keyQuery.CacheQuery                      = true;
            keyQuery.AbsoluteExpirationRelativeToNow = absoluteExpirationRelativeToNow;
            keyQuery.CacheKey                        = cacheKey;

            if (this.memoryCache != null) {
                if (this.memoryCache.TryGetValue(cacheKey, out object[][] rows)) {
                    this.resultCache.Add(keyQuery, rows);
                    this.executedQueries.Add(keyQuery);
                    return;
                }
            }

            if (this.distributedCache != null) {
                var cacheBuffer = await this.distributedCache.GetAsync(cacheKey, cancellationToken);
                if (cacheBuffer != null) {
                    var cacheRow = this.cacheSerializer.Deserialize<object[][]>(cacheBuffer);
                    if (cacheRow != null) {
                        if (this.memoryCache != null) {
                            if (absoluteExpirationRelativeToNow.HasValue) {
                                this.memoryCache.Set(cacheKey, cacheRow, absoluteExpirationRelativeToNow.Value);
                            }
                            else {
                                this.memoryCache.Set(cacheKey, cacheRow);
                            }
                        }

                        this.resultCache.Add(keyQuery, cacheRow);
                        this.executedQueries.Add(keyQuery);
                    }
                }
            }
        }

        public async ValueTask VisitMultipleKeyQueryAsync<TEntity, TKey>(MultipleKeyQuery<TEntity, TKey> multipleKeyQuery, CancellationToken cancellationToken = default)
            where TEntity : class {
            if (multipleKeyQuery.Keys.Length == 0) {
                this.resultCache.Add(multipleKeyQuery, new List<object[]>(0));
                this.executedQueries.Add(multipleKeyQuery);
                return;
            }

            if (!this.cacheOptions.TryGetCacheOptions(multipleKeyQuery.Collection.CollectionName, out var collectionCacheOptions)) {
                return;
            }

            var resultTasks = new List<ValueTask<(TKey key, string cacheKey, object[][] rows)>>();
            foreach (var key in multipleKeyQuery.Keys) {
                resultTasks.Add(TryGetKeyAsync(key));
            }

            var results = new List<object[]>(multipleKeyQuery.Keys.Length);
            var matchedKeys = new HashSet<TKey>(multipleKeyQuery.Keys.Length);
            var unmatchedKeys = new HashSet<TKey>(multipleKeyQuery.Keys.Length);
            string firstUnmatchedKey = null;
            foreach (var resultTask in resultTasks) {
                var (key, cacheKey, result) = await resultTask;
                if (result == null || result.Length == 0) {
                    unmatchedKeys.Add(key);
                    firstUnmatchedKey ??= cacheKey;
                }
                else {
                    results.Add(result[0]);
                    matchedKeys.Add(key);
                }
            }

            if (matchedKeys.Count == 0) {
                multipleKeyQuery.CacheQuery = true;
                multipleKeyQuery.CacheKey = firstUnmatchedKey;
                multipleKeyQuery.AbsoluteExpirationRelativeToNow = collectionCacheOptions.AbsoluteExpirationRelativeToNow;
                return;
            }

            if (matchedKeys.Count != multipleKeyQuery.Keys.Length) {
                var executedQuery = new MultipleKeyQuery<TEntity, TKey>([.. matchedKeys], multipleKeyQuery.Collection);
                var remainingQuery = new MultipleKeyQuery<TEntity, TKey>([.. unmatchedKeys], multipleKeyQuery.Collection) {
                    CacheQuery = true,
                    CacheKey = firstUnmatchedKey,
                    AbsoluteExpirationRelativeToNow = collectionCacheOptions.AbsoluteExpirationRelativeToNow
                };
                this.resultCache.Add(executedQuery, results);
                this.partiallyExecutedQueries.Add(multipleKeyQuery, (executedQuery, remainingQuery));
            }
            else {
                this.resultCache.Add(multipleKeyQuery, results);
                this.executedQueries.Add(multipleKeyQuery);
            }

            async ValueTask<(TKey key, string cacheKey, object[][] rows)> TryGetKeyAsync(TKey key) {
                var cacheKey = collectionCacheOptions.CacheKeyProvider.GetEntityCacheKey<TEntity, TKey>(multipleKeyQuery.Collection, key);
                if (string.IsNullOrWhiteSpace(cacheKey)) {
                    return (key, cacheKey, null);
                }

                if (this.memoryCache != null) {
                    if (this.memoryCache.TryGetValue(cacheKey, out object[][] rows)) {
                        return (key, cacheKey, rows);
                    }
                }

                if (this.distributedCache != null) {
                    var cacheBuffer = await this.distributedCache.GetAsync(cacheKey, cancellationToken);
                    if (cacheBuffer != null) {
                        var cacheRow = this.cacheSerializer.Deserialize<object[][]>(cacheBuffer);
                        if (cacheRow != null) {
                            if (this.memoryCache != null) {
                                if (collectionCacheOptions.AbsoluteExpirationRelativeToNow.HasValue) {
                                    this.memoryCache.Set(cacheKey, cacheRow, collectionCacheOptions.AbsoluteExpirationRelativeToNow.Value);
                                }
                                else {
                                    this.memoryCache.Set(cacheKey, cacheRow);
                                }
                            }

                            return (key, cacheKey, cacheRow);
                        }
                    }
                }

                return (key, cacheKey, null);
            }
        }
    }
}