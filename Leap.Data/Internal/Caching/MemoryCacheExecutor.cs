namespace Leap.Data.Internal.Caching {
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;

    using Leap.Data.Queries;

    class MemoryCacheExecutor : ICacheExecutor, IQueryVisitor {
        private readonly IMemoryCache memoryCache;

        private readonly ResultCache resultCache;

        private readonly HashSet<Guid> executedQueryIds = new();

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
            if (this.memoryCache.TryGetValue(keyQuery.Key, out object[] row)) {
                this.resultCache.Add(keyQuery, new List<object[]> { row });
                this.executedQueryIds.Add(keyQuery.Identifier);
            }
        }

        public void VisitMultipleKeyQuery<TEntity, TKey>(MultipleKeyQuery<TEntity, TKey> multipleKeyQuery)
            where TEntity : class {
            var result = new List<object[]>();
            foreach (var key in multipleKeyQuery.Keys) {
                if (!this.memoryCache.TryGetValue(key, out object[] row)) {
                    return; // can't support this query as don't have all the entities cached
                }
                
                result.Add(row);
            }

            this.resultCache.Add(multipleKeyQuery, result);
            this.executedQueryIds.Add(multipleKeyQuery.Identifier);
        }

        public ValueTask<ExecuteResult> ExecuteAsync(IEnumerable<IQuery> queries, CancellationToken cancellationToken = default) {
            this.executedQueryIds.Clear();
            foreach (var query in queries) {
                query.Accept(this);
            }

            return ValueTask.FromResult(
                new ExecuteResult(queries.Where(q => this.executedQueryIds.Contains(q.Identifier)), queries.Where(q => !this.executedQueryIds.Contains(q.Identifier))));
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