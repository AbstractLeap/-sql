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
            var result = new List<object[]>();
            foreach (var key in multipleKeyQuery.Keys) {
                if (!this.memoryCache.TryGetValue(CacheKeyProvider.GetCacheKey<TEntity, TKey>(multipleKeyQuery.Collection, key), out object[] row)) {
                    return; // can't support this query as don't have all the entities cached
                }
                
                result.Add(row);
            }

            this.resultCache.Add(multipleKeyQuery, result);
            this.executedQueries.Add(multipleKeyQuery);
        }

        public ValueTask<ExecuteResult> ExecuteAsync(IEnumerable<IQuery> queries, CancellationToken cancellationToken = default) {
            this.executedQueries.Clear();
            foreach (var query in queries) {
                query.Accept(this);
            }

            return ValueTask.FromResult(new ExecuteResult(queries.Where(q => this.executedQueries.Contains(q)), queries.Where(q => !this.executedQueries.Contains(q))));
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