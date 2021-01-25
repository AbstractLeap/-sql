namespace Leap.Data.Internal.Caching {
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;

    using Leap.Data.Queries;

    class DistributedCacheExecutor : ICacheExecutor, IAsyncQueryVisitor {
        private readonly IDistributedCache distributedCache;

        private readonly ResultCache resultCache;

        private readonly HashSet<Guid> executedQueryIds = new();

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
            var cachedRow = await this.distributedCache.GetAsync<object[]>(keyQuery.Key, cancellationToken);
            if (cachedRow != null) {
                this.resultCache.Add(keyQuery, new List<object[]> { cachedRow });
                this.executedQueryIds.Add(keyQuery.Identifier);
            }
        }

        public ValueTask VisitMultipleKeyQueryAsync<TEntity, TKey>(MultipleKeyQuery<TEntity, TKey> multipleKeyQuery, CancellationToken cancellationToken = default)
            where TEntity : class {
            throw new NotImplementedException();
        }

        public async ValueTask<ExecuteResult> ExecuteAsync(IEnumerable<IQuery> queries, CancellationToken cancellationToken = default) {
            this.executedQueryIds.Clear();
            foreach (var query in queries) {
                await query.AcceptAsync(this, cancellationToken);
            }

            return new ExecuteResult(queries.Where(q => this.executedQueryIds.Contains(q.Identifier)), queries.Where(q => !this.executedQueryIds.Contains(q.Identifier)));
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