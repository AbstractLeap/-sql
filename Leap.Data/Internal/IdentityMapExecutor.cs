namespace Leap.Data.Internal {
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

    class IdentityMapExecutor {
        private readonly IdentityMap identityMap;

        private readonly ResultCache resultCache;

        public IdentityMapExecutor(IdentityMap identityMap) {
            this.identityMap = identityMap;
            this.resultCache = new ResultCache();
        }

        private ValueTask<Maybe> ExecuteAsync(IQuery query) {
            var queryType = query.GetType();
            var genericTypeDefinition = queryType.GetGenericTypeDefinition();
            if (genericTypeDefinition == typeof(KeyQuery<,>)) {
                return (ValueTask<Maybe>)this.CallMethod(queryType.GetGenericArguments(), nameof(this.TryGetInstanceFromIdentityMap), query);
            }

            return new ValueTask<Maybe>(Maybe.NotSuccessful);
        }

        private ValueTask<Maybe> TryGetInstanceFromIdentityMap<TEntity, TKey>(KeyQuery<TEntity, TKey> keyQuery)
            where TEntity : class {
            if (this.identityMap.TryGetValue(keyQuery.Key, out IDocument<TEntity> document)) {
                if (document.State == DocumentState.Deleted) {
                    // we need to say that we've handled it but that we return nothing
                    return new ValueTask<Maybe>(new Maybe(new List<IDocument<TEntity>>()));
                }
                
                return new ValueTask<Maybe>(new Maybe(new List<IDocument<TEntity>> { document }));
            }

            return new ValueTask<Maybe>(Maybe.NotSuccessful);
        }

        public async ValueTask<ExecuteResult> ExecuteAsync(IEnumerable<IQuery> queries, CancellationToken cancellationToken = default) {
            var executedQueries = new List<IQuery>();
            var nonExecutedQueries = new List<IQuery>();
            foreach (var query in queries) {
                var executionResult = await this.ExecuteAsync(query);
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