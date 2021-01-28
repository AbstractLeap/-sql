namespace Leap.Data.Internal {
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;

    using Leap.Data.IdentityMap;
    using Leap.Data.Queries;

    class IdentityMapExecutor : IQueryVisitor {
        private readonly IdentityMap identityMap;

        private readonly ResultCache resultCache;

        private readonly HashSet<Guid> executedQueryIds = new();

        public IdentityMapExecutor(IdentityMap identityMap) {
            this.identityMap = identityMap;
            this.resultCache = new ResultCache();
        }

        public void VisitEntityQuery<TEntity>(EntityQuery<TEntity> entityQuery)
            where TEntity : class {
            // not supported by this
        }

        public void VisitKeyQuery<TEntity, TKey>(KeyQuery<TEntity, TKey> keyQuery)
            where TEntity : class {
            if (this.identityMap.TryGetValue(keyQuery.Key, out IDocument<TEntity> document)) {
                if (document.State == DocumentState.Deleted) {
                    // we need to say that we've handled it but that we return nothing
                    this.resultCache.Add(keyQuery, new List<IDocument<TEntity>>());
                }
                else {
                    this.resultCache.Add(keyQuery, new List<IDocument<TEntity>> { document });
                }

                this.executedQueryIds.Add(keyQuery.Identifier);
            }
        }

        public void VisitMultipleKeyQuery<TEntity, TKey>(MultipleKeyQuery<TEntity, TKey> multipleKeyQuery)
            where TEntity : class {
            var result = new List<IDocument<TEntity>>();
            foreach (var key in multipleKeyQuery.Keys) {
                if (!this.identityMap.TryGetValue(key, out IDocument<TEntity> document)) {
                    return;
                }

                if (document.State != DocumentState.Deleted) {
                    result.Add(document);
                }
            }
            
            this.resultCache.Add(multipleKeyQuery, result);
            this.executedQueryIds.Add(multipleKeyQuery.Identifier);
        }

        public ExecuteResult Execute(IEnumerable<IQuery> queries, CancellationToken cancellationToken = default) {
            this.executedQueryIds.Clear();
            foreach (var query in queries) {
                query.Accept(this);
            }

            return new ExecuteResult(queries.Where(q => this.executedQueryIds.Contains(q.Identifier)), queries.Where(q => !this.executedQueryIds.Contains(q.Identifier)));
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