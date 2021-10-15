namespace Leap.Data.Internal {
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;

    using Leap.Data.IdentityMap;
    using Leap.Data.Queries;
    using Leap.Data.UnitOfWork;

    class IdentityMapExecutor : IQueryVisitor {
        private readonly IdentityMap identityMap;

        private readonly UnitOfWork unitOfWork;

        private readonly ResultCache resultCache;

        private readonly HashSet<IQuery> executedQueries = new();

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
            foreach (var key in multipleKeyQuery.Keys) {
                if (!this.identityMap.TryGetValue(key, out TEntity entity)) {
                    return;
                }

                var state = this.unitOfWork.GetState(multipleKeyQuery.Collection, entity);
                if (state == DocumentState.NotAttached) {
                    return;
                }
                
                if (state != DocumentState.Deleted) {
                    result.Add(entity);
                }
            }

            this.resultCache.Add(multipleKeyQuery, result);
            this.executedQueries.Add(multipleKeyQuery);
        }

        public ExecuteResult Execute(IEnumerable<IQuery> queries, CancellationToken cancellationToken = default) {
            this.executedQueries.Clear();
            foreach (var query in queries) {
                query.Accept(this);
            }

            return new ExecuteResult(queries.Where(q => this.executedQueries.Contains(q)), queries.Where(q => !this.executedQueries.Contains(q)));
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