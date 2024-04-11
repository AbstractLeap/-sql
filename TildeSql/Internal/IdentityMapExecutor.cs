namespace TildeSql.Internal {
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;

    using TildeSql.IdentityMap;
    using TildeSql.Queries;
    using TildeSql.UnitOfWork;

    class IdentityMapExecutor : IQueryVisitor {
        private readonly IdentityMap identityMap;

        private readonly UnitOfWork unitOfWork;

        private readonly ResultCache resultCache;

        private readonly HashSet<IQuery> executedQueries = new();

        private readonly Dictionary<IQuery, (IQuery Executed, IQuery Remaining)> partiallyExecutedQueries = new();

        public IdentityMapExecutor(IdentityMap identityMap, UnitOfWork unitOfWork) {
            this.identityMap = identityMap;
            this.unitOfWork  = unitOfWork;
            this.resultCache = new ResultCache();
        }

        public void VisitEntityQuery<TEntity>(EntityQuery<TEntity> entityQuery)
            where TEntity : class {
            // not supported by this
        }

        public void VisitKeyQuery<TEntity, TKey>(KeyQuery<TEntity, TKey> keyQuery)
            where TEntity : class {
            if (this.identityMap.TryGetValue(keyQuery.Key, out TEntity entity)) {
                var state = this.unitOfWork.GetState(keyQuery.Collection, entity);
                switch (state) {
                    case DocumentState.NotAttached:
                        return;
                    case DocumentState.Deleted:
                        // we need to say that we've handled it but that we return nothing
                        this.resultCache.Add(keyQuery, new List<TEntity>());
                        break;
                    default:
                        this.resultCache.Add(keyQuery, new List<TEntity> { entity });
                        break;
                }

                this.executedQueries.Add(keyQuery);
            }
        }

        public void VisitMultipleKeyQuery<TEntity, TKey>(MultipleKeyQuery<TEntity, TKey> multipleKeyQuery)
            where TEntity : class {
            var result = new List<TEntity>();
            var matchedKeys = new HashSet<TKey>();
            var unmatchedKeys = new HashSet<TKey>();
            foreach (var key in multipleKeyQuery.Keys) {
                if (!this.identityMap.TryGetValue(key, out TEntity entity)) {
                    unmatchedKeys.Add(key);
                    continue;
                }

                var state = this.unitOfWork.GetState(multipleKeyQuery.Collection, entity);
                if (state is DocumentState.NotAttached or DocumentState.Deleted) {
                    unmatchedKeys.Add(key);
                    continue;
                }

                result.Add(entity);
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

        public ExecuteResult Execute(IEnumerable<IQuery> queries, CancellationToken cancellationToken = default) {
            this.executedQueries.Clear();
            this.partiallyExecutedQueries.Clear();

            foreach (var query in queries) {
                query.Accept(this);
            }

            var executed = queries.Where(q => this.executedQueries.Contains(q));
            var partiallyExecuted = this.partiallyExecutedQueries.Where(q => queries.Contains(q.Key)).Select(q => (q.Key, q.Value.Executed, q.Value.Remaining));
            var nonExecutedQueries = queries.Where(q => !this.executedQueries.Contains(q) && !this.partiallyExecutedQueries.ContainsKey(q));
            return new ExecuteResult(executed, partiallyExecuted, nonExecutedQueries);
        }

        public IAsyncEnumerable<TEntity> GetAsync<TEntity>(IQuery query)
            where TEntity : class {
            return Get<TEntity>(query).ToAsyncEnumerable();
        }

        private IEnumerable<TEntity> Get<TEntity>(IQuery query)
            where TEntity : class {
            if (this.resultCache.TryGetValue<TEntity>(query, out var result)) {
                foreach (var entity in result) {
                    yield return entity;
                }
            }
            else {
                throw new Exception($"{nameof(IdentityMapExecutor)} did not execute {query} so can not get result");
            }
        }
    }
}