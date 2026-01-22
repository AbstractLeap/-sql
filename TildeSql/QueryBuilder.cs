namespace TildeSql {
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;

    using TildeSql.Queries;
    using TildeSql.Schema;

    internal class QueryBuilder<TEntity> : IQueryBuilder<TEntity>
        where TEntity : class {
        private readonly Session session;

        private readonly Collection collection;

        public QueryBuilder(Session session, Collection collection) {
            this.session = session;
            this.collection   = collection;
        }

        public ValueTask<TEntity> SingleAsync<TKey>(TKey key, bool disableCache = false, CancellationToken cancellationToken = default) {
            var query = new KeyQuery<TEntity, TKey>(key, this.collection);
            if (disableCache) {
                query.DisableCache();
            }

            var queryEngine = this.session.GetEngine();
            return queryEngine.GetResult<TEntity>(query).SingleOrDefaultAsync(cancellationToken);
        }

        public IFutureSingleResult<TEntity, TKey> SingleFuture<TKey>(TKey key, bool disableCache = false) {
            var query = new KeyQuery<TEntity, TKey>(key, this.collection);
            if (disableCache) {
                query.DisableCache();
            }

            var queryEngine = this.session.GetEngine();
            queryEngine.Add(query);
            return new FutureSingleResult<TEntity, TKey>(query, this.session);
        }

        public IAsyncEnumerable<TEntity> MultipleAsync<TKey>(IEnumerable<TKey> keys, bool disableCache = false) {
            var query = new MultipleKeyQuery<TEntity, TKey>(keys as TKey[] ?? keys.ToArray(), this.collection);
            if (disableCache) {
                query.DisableCache();
            }

            var queryEngine = this.session.GetEngine();
            return queryEngine.GetResult<TEntity>(query);
        }

        public IFutureMultipleResult<TEntity, TKey> MultipleFuture<TKey>(IEnumerable<TKey> keys, bool disableCache = false) {
            var query = new MultipleKeyQuery<TEntity, TKey>(keys as TKey[] ?? keys.ToArray(), this.collection);
            if (disableCache) {
                query.DisableCache();
            }

            var queryEngine = this.session.GetEngine();
            queryEngine.Add(query);
            return new FutureMultipleResult<TEntity, TKey>(query, this.session);
        }

        public IAsyncEnumerator<TEntity> GetAsyncEnumerator(CancellationToken cancellationToken = new CancellationToken()) {
            var query = new EntityQuery<TEntity>(this.collection);
            var queryEngine = this.session.GetEngine();
            return queryEngine.GetResult<TEntity>(query).GetAsyncEnumerator(cancellationToken);
        }

        public IEntityQueryBuilder<TEntity> Where(string whereClause, IDictionary<string, object> parameters = null) {
            var entityQueryBuilder = new EntityQueryBuilder<TEntity>(this.session, this.collection);
            entityQueryBuilder.Where(whereClause, parameters);
            return entityQueryBuilder;
        }
        
        public IEntityQueryBuilder<TEntity> Where(string whereClause, object parameters) {
            var entityQueryBuilder = new EntityQueryBuilder<TEntity>(this.session, this.collection);
            entityQueryBuilder.Where(whereClause, parameters);
            return entityQueryBuilder;
        }

        public IEntityQueryBuilder<TEntity> OrderBy(string orderByClause) {
            var entityQueryBuilder = new EntityQueryBuilder<TEntity>(this.session, this.collection);
            entityQueryBuilder.OrderBy(orderByClause);
            return entityQueryBuilder;
        }

        public IEntityQueryBuilder<TEntity> Offset(int offset) {
            var entityQueryBuilder = new EntityQueryBuilder<TEntity>(this.session, this.collection);
            entityQueryBuilder.Offset(offset);
            return entityQueryBuilder;
        }

        public IEntityQueryBuilder<TEntity> Limit(int limit) {
            var entityQueryBuilder = new EntityQueryBuilder<TEntity>(this.session, this.collection);
            entityQueryBuilder.Limit(limit);
            return entityQueryBuilder;
        }

        public IEntityQueryBuilder<TEntity> Limit(int limit, out ICountAccessor countAccessor) {
            var entityQueryBuilder = new EntityQueryBuilder<TEntity>(this.session, this.collection);
            entityQueryBuilder.Limit(limit, out countAccessor);
            return entityQueryBuilder;
        }

        public IEntityQueryBuilder<TEntity> Cache(TimeSpan? absoluteExpirationRelativeToNow = null, string key = null) {
            var entityQueryBuilder = new EntityQueryBuilder<TEntity>(this.session, this.collection);
            entityQueryBuilder.Cache(absoluteExpirationRelativeToNow, key);
            return entityQueryBuilder;
        }

        public IEntityQueryBuilder<TEntity> NoCache() {
            var entityQueryBuilder = new EntityQueryBuilder<TEntity>(this.session, this.collection);
            entityQueryBuilder.NoCache();
            return entityQueryBuilder;
        }

        public IFutureEntityQueryResult<TEntity> Future() {
            var query = new EntityQuery<TEntity>(this.collection);
            var queryEngine = this.session.GetEngine();
            queryEngine.Add(query);
            return new FutureEntityQueryResult<TEntity>(query, this.session);
        }
    }
}