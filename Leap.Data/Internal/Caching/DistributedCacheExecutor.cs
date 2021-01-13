namespace Leap.Data.Internal.Caching {
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;

    using Fasterflect;

    using Leap.Data.IdentityMap;
    using Leap.Data.Queries;
    using Leap.Data.Utilities;

    class DistributedCacheExecutor : ICacheExecutor {
        private readonly IDistributedCache distributedCache;

        private readonly ResultCache resultCache;

        public DistributedCacheExecutor(IDistributedCache distributedCache) {
            this.distributedCache = distributedCache;
            this.resultCache      = new ResultCache();
        }

        private ValueTask<Maybe> ExecuteAsync(IQuery query, CancellationToken cancellationToken) {
            var queryType = query.GetType();
            var genericTypeDefinition = queryType.GetGenericTypeDefinition();
            if (genericTypeDefinition == typeof(KeyQuery<,>)) {
                return (ValueTask<Maybe>)this.CallMethod(queryType.GetGenericArguments(), nameof(this.TryGetInstanceFromCache), query, cancellationToken);
            }

            return new ValueTask<Maybe>(Maybe.NotSuccessful);
        }

        private async ValueTask<Maybe> TryGetInstanceFromCache<TEntity, TKey>(KeyQuery<TEntity, TKey> keyQuery, CancellationToken cancellationToken)
            where TEntity : class {
            var cachedDocument = await this.distributedCache.GetAsync<IDocument<TEntity>>(keyQuery.Key, cancellationToken);
            if (cachedDocument != null) {
                return new Maybe(new List<IDocument<TEntity>> { cachedDocument });
            }
            
            return Maybe.NotSuccessful;
        }

        public async ValueTask<ExecuteResult> ExecuteAsync(IEnumerable<IQuery> queries, CancellationToken cancellationToken = default) {
            var executedQueries = new List<IQuery>();
            var nonExecutedQueries = new List<IQuery>();
            foreach (var query in queries) {
                var executionResult = await this.ExecuteAsync(query, cancellationToken);
                if (!executionResult.WasSuccessful) {
                    nonExecutedQueries.Add(query);
                    continue;
                }

                this.resultCache.Add(query, (IList)executionResult.Result);
                executedQueries.Add(query);
            }

            return new ExecuteResult(executedQueries, nonExecutedQueries);
        }

        public IAsyncEnumerable<IDocument<TEntity>> GetAsync<TEntity>(IQuery query)
            where TEntity : class {
            return Get<TEntity>(query).ToAsyncEnumerable();
        }

        private IEnumerable<IDocument<TEntity>> Get<TEntity>(IQuery query)
            where TEntity : class {
            if (this.resultCache.TryGetValue<IDocument<TEntity>>(query, out var result)) {
                foreach (var document in result) {
                    yield return document;
                }
            }
            else {
                throw new Exception($"{nameof(IdentityMapExecutor)} did not execute {query} so can not get result");
            }
        }
    }
}