namespace TildeSql.Internal.Caching {
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;

    using TildeSql.Queries;

    class MemoryCacheExecutor : ICacheExecutor, IQueryVisitor {
        private readonly IMemoryCache memoryCache;

        private readonly ResultCache resultCache;

        private readonly HashSet<IQuery> executedQueries = new();

        private readonly Dictionary<IQuery, (IQuery Executed, IQuery Remaining)> partiallyExecutedQueries = new();

        public MemoryCacheExecutor(IMemoryCache memoryCache) {
            this.memoryCache = memoryCache;
            this.resultCache = new ResultCache();
        }

        public void VisitEntityQuery<TEntity>(EntityQuery<TEntity> entityQuery)
            where TEntity : class {
            // not supported by this
        }

        public void VisitKeyQuery<TEntity, TKey>(KeyQuery<TEntity, TKey> keyQuery)
            where TEntity : class {
            if (this.memoryCache.TryGetValue(CacheKeyProvider.GetCacheKey<TEntity, TKey>(keyQuery.Collection, keyQuery.Key), out object[] row)) {
                this.resultCache.Add(keyQuery, new List<object[]> { row });
                this.executedQueries.Add(keyQuery);
            }
        }

        public void VisitMultipleKeyQuery<TEntity, TKey>(MultipleKeyQuery<TEntity, TKey> multipleKeyQuery)
            where TEntity : class {
            var result = new List<object[]>(multipleKeyQuery.Keys.Length);
            var matchedKeys = new HashSet<TKey>(multipleKeyQuery.Keys.Length);
            var unmatchedKeys = new HashSet<TKey>(multipleKeyQuery.Keys.Length);
            foreach (var key in multipleKeyQuery.Keys) {
                if (this.memoryCache.TryGetValue(CacheKeyProvider.GetCacheKey<TEntity, TKey>(multipleKeyQuery.Collection, key), out object[] row)) {
                    unmatchedKeys.Add(key);
                    continue;
                }

                result.Add(row);
                matchedKeys.Add(key);
            }

            if (matchedKeys.Count == 0) return;
            if (matchedKeys.Count != multipleKeyQuery.Keys.Length) {
                var executedQuery = new MultipleKeyQuery<TEntity, TKey>([.. matchedKeys], multipleKeyQuery.Collection);
                var remainingQuery = new MultipleKeyQuery<TEntity, TKey>([..unmatchedKeys], multipleKeyQuery.Collection);
                this.resultCache.Add(executedQuery, result);
                this.partiallyExecutedQueries.Add(multipleKeyQuery, (executedQuery, remainingQuery));
            }
            else {
                this.resultCache.Add(multipleKeyQuery, result);
                this.executedQueries.Add(multipleKeyQuery);
            }
        }

        public ValueTask<ExecuteResult> ExecuteAsync(IList<IQuery> queries, CancellationToken cancellationToken = default) {
            this.executedQueries.Clear();
            this.partiallyExecutedQueries.Clear();

            foreach (var query in queries) {
                query.Accept(this);
            }

            var executed = queries.Where(q => this.executedQueries.Contains(q));
            var partiallyExecuted = this.partiallyExecutedQueries.Where(q => queries.Contains(q.Key)).Select(q => (q.Key, q.Value.Executed, q.Value.Remaining));
            var nonExecutedQueries = queries.Where(q => !this.executedQueries.Contains(q) && !this.partiallyExecutedQueries.ContainsKey(q));
            return ValueTask.FromResult(new ExecuteResult(executed, partiallyExecuted, nonExecutedQueries));
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
                throw new Exception($"{nameof(MemoryCacheExecutor)} did not execute {query} so can not get result");
            }
        }
    }
}