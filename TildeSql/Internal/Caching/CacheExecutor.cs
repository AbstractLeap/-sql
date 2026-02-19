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
            await this.TryFindInCacheAsync(entityQuery, ckp => ckp.GetEntityQueryCacheKey(entityQuery.Collection, entityQuery), cancellationToken);
        }

        public async ValueTask VisitKeyQueryAsync<TEntity, TKey>(KeyQuery<TEntity, TKey> keyQuery, CancellationToken cancellationToken = default)
            where TEntity : class {
            await this.TryFindInCacheAsync(keyQuery, ckp => ckp.GetEntityCacheKey<TEntity, TKey>(keyQuery.Collection, keyQuery.Key), cancellationToken);
        }

        private async Task TryFindInCacheAsync<TEntity>(QueryBase<TEntity> query, Func<ICacheKeyProvider, string> calculatedCacheKeyFunc, CancellationToken cancellationToken)
            where TEntity : class {
            if (query.IsCacheDisabled) { // caching disabled via query
                return;
            }

            CollectionCacheOptions collectionCacheOptions = null;
            if (query.ExplicitCacheKey == null || !query.ExplicitAbsoluteExpirationRelativeToNow.HasValue) {
                if (!this.cacheOptions.TryGetCacheOptions(query.Collection.CollectionName, out collectionCacheOptions)) {
                    return;
                }
            }

            var cacheKey = query.ExplicitCacheKey ?? calculatedCacheKeyFunc(collectionCacheOptions.CacheKeyProvider);
            var absoluteExpirationRelativeToNow = query.ExplicitAbsoluteExpirationRelativeToNow ?? collectionCacheOptions?.AbsoluteExpirationRelativeToNow;
            if (string.IsNullOrWhiteSpace(cacheKey) || !absoluteExpirationRelativeToNow.HasValue) {
                return;
            }

            query.AddResolvedCacheOptions(cacheKey, absoluteExpirationRelativeToNow.Value);
            if (this.memoryCache != null) {
                if (this.memoryCache.TryGetValue(cacheKey, out object[][] rows)) {
                    this.resultCache.Add(query, rows);
                    this.executedQueries.Add(query);
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

                        this.resultCache.Add(query, cacheRow);
                        this.executedQueries.Add(query);
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

            if (multipleKeyQuery.IsCacheDisabled) return;
            if (!this.cacheOptions.TryGetCacheOptions(multipleKeyQuery.Collection.CollectionName, out var collectionCacheOptions)) { // NOTE it doesn't make sense to support explicit caching keys for multikey queries
                return;
            }

            var resultTasks = new List<ValueTask<(TKey key, string cacheKey, object[][] rows)>>();
            foreach (var key in multipleKeyQuery.Keys) {                
                resultTasks.Add(TryGetKeyAsync(key));
            }

            var results = new List<object[]>(multipleKeyQuery.Keys.Length);
            var matchedKeys = new HashSet<(TKey key, string cacheKey)>(multipleKeyQuery.Keys.Length);
            var unmatchedKeys = new HashSet<(TKey key, string cacheKey)>(multipleKeyQuery.Keys.Length);
            foreach (var resultTask in resultTasks) {
                var (key, cacheKey, result) = await resultTask;
                multipleKeyQuery.AddResolvedCacheOptions(cacheKey, collectionCacheOptions.AbsoluteExpirationRelativeToNow);
                if (result == null || result.Length == 0) {
                    unmatchedKeys.Add((key, cacheKey));
                }
                else {
                    results.Add(result[0]);
                    matchedKeys.Add((key, cacheKey));
                }
            }

            if (matchedKeys.Count == 0) {
                return;
            }

            if (matchedKeys.Count != multipleKeyQuery.Keys.Length) {
                var tracked = multipleKeyQuery.Tracked;
                var disableCaching = multipleKeyQuery.IsCacheDisabled;
                var executedQuery = new MultipleKeyQuery<TEntity, TKey>(matchedKeys.Select(t => t.key).ToArray(), multipleKeyQuery.Collection, tracked);
                if (disableCaching) executedQuery.DisableCache();

                foreach (var k in matchedKeys) {
                    executedQuery.AddResolvedCacheOptions(k.cacheKey, collectionCacheOptions.AbsoluteExpirationRelativeToNow);
                }

                var remainingQuery = new MultipleKeyQuery<TEntity, TKey>(unmatchedKeys.Select(t => t.key).ToArray(), multipleKeyQuery.Collection, tracked);
                if (disableCaching) remainingQuery.DisableCache();

                foreach (var k in unmatchedKeys) {
                    remainingQuery.AddResolvedCacheOptions(k.cacheKey, collectionCacheOptions.AbsoluteExpirationRelativeToNow);
                }

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
                                 this.memoryCache.Set(cacheKey, cacheRow, collectionCacheOptions.AbsoluteExpirationRelativeToNow);
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