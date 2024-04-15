namespace TildeSql.Internal.Caching {
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;

    using TildeSql.Queries;

    class DistributedCacheExecutor : ICacheExecutor, IAsyncQueryVisitor {
        private readonly IDistributedCache distributedCache;

        private readonly ResultCache resultCache;

        private readonly HashSet<IQuery> executedQueries = new();

        private readonly Dictionary<IQuery, (IQuery Executed, IQuery Remaining)> partiallyExecutedQueries = new();

        public DistributedCacheExecutor(IDistributedCache distributedCache) {
            this.distributedCache = distributedCache;
            this.resultCache      = new ResultCache();
        }

        public ValueTask VisitEntityQueryAsync<TEntity>(EntityQuery<TEntity> entityQuery, CancellationToken cancellationToken = default)
            where TEntity : class {
            // not supported by this
            return ValueTask.CompletedTask;
        }

        public async ValueTask VisitKeyQueryAsync<TEntity, TKey>(KeyQuery<TEntity, TKey> keyQuery, CancellationToken cancellationToken = default)
            where TEntity : class {
            var cachedRow = await this.distributedCache.GetAsync<object[]>(CacheKeyProvider.GetCacheKey<TEntity, TKey>(keyQuery.Collection, keyQuery.Key), cancellationToken);
            if (cachedRow != null) {
                this.resultCache.Add(keyQuery, new List<object[]> { cachedRow });
                this.executedQueries.Add(keyQuery);
            }
        }

        public async ValueTask VisitMultipleKeyQueryAsync<TEntity, TKey>(MultipleKeyQuery<TEntity, TKey> multipleKeyQuery, CancellationToken cancellationToken = default)
            where TEntity : class {
            if (multipleKeyQuery.Keys.Length == 0) {
                this.resultCache.Add(multipleKeyQuery, new List<object[]>(0));
                this.executedQueries.Add(multipleKeyQuery);
            }

            var resultTasks = multipleKeyQuery.Keys.Select(
                async key =>
                    (Key: key,
                        Result: await this.distributedCache.GetAsync<object[]>(CacheKeyProvider.GetCacheKey<TEntity, TKey>(multipleKeyQuery.Collection, key), cancellationToken)));

            var results = new List<object[]>(multipleKeyQuery.Keys.Length);
            var matchedKeys = new HashSet<TKey>(multipleKeyQuery.Keys.Length);
            var unmatchedKeys = new HashSet<TKey>(multipleKeyQuery.Keys.Length);
            foreach (var resultTask in resultTasks) {
                var (key, result) = await resultTask;
                if (result == null) {
                    unmatchedKeys.Add(key);
                }
                else {
                    results.Add(result);
                    matchedKeys.Add(key);
                }
            }

            if (matchedKeys.Count != multipleKeyQuery.Keys.Length) {
                var executedQuery = new MultipleKeyQuery<TEntity, TKey>([.. matchedKeys], multipleKeyQuery.Collection);
                var remainingQuery = new MultipleKeyQuery<TEntity, TKey>([..unmatchedKeys], multipleKeyQuery.Collection);
                this.resultCache.Add(executedQuery, results);
                this.partiallyExecutedQueries.Add(multipleKeyQuery, (executedQuery, remainingQuery));
            }
            else {
                this.resultCache.Add(multipleKeyQuery, results);
                this.executedQueries.Add(multipleKeyQuery);
            }
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
                throw new Exception($"{nameof(DistributedCacheExecutor)} did not execute {query} so can not get result");
            }
        }
    }
}