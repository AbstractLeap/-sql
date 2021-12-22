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
            var resultTasks = new List<ValueTask<object[]>>();
            foreach (var key in multipleKeyQuery.Keys) {
                resultTasks.Add(this.distributedCache.GetAsync<object[]>(CacheKeyProvider.GetCacheKey<TEntity, TKey>(multipleKeyQuery.Collection, key), cancellationToken));
            }

            var hasAllResults = true;
            foreach (var resultTask in resultTasks) {
                var result = await resultTask;
                if (result == null) {
                    hasAllResults = false;
                }
            }

            if (hasAllResults) {
                this.resultCache.Add(multipleKeyQuery, resultTasks.Select(t => t.Result).ToList());
                this.executedQueries.Add(multipleKeyQuery);
            }
        }

        public async ValueTask<ExecuteResult> ExecuteAsync(IEnumerable<IQuery> queries, CancellationToken cancellationToken = default) {
            this.executedQueries.Clear();
            foreach (var query in queries) {
                await query.AcceptAsync(this, cancellationToken);
            }

            return new ExecuteResult(queries.Where(q => this.executedQueries.Contains(q)), queries.Where(q => !this.executedQueries.Contains(q)));
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