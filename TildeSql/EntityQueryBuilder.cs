namespace TildeSql {
    using System;
    using System.Collections.Generic;
    using System.Threading;

    using TildeSql.Utilities;

    using TildeSql.Queries;
    using TildeSql.Schema;

    class EntityQueryBuilder<TEntity> : IEntityQueryBuilder<TEntity>
        where TEntity : class {
        private readonly Session session;

        private readonly EntityQuery<TEntity> query;

        public EntityQueryBuilder(Session session, Collection collection) {
            this.session = session;
            this.query   = new EntityQuery<TEntity>(collection);
        }

        public IAsyncEnumerator<TEntity> GetAsyncEnumerator(CancellationToken cancellationToken = new CancellationToken()) {
            var queryEngine = this.session.GetEngine();
            return queryEngine.GetResult<TEntity>(this.query).GetAsyncEnumerator(cancellationToken);
        }

        public IEntityQueryBuilder<TEntity> Where(string whereClause, IDictionary<string, object> parameters = null) {
            // TODO multiple invocations?
            this.query.WhereClause           = whereClause;
            this.query.WhereClauseParameters = parameters;
            return this;
        }

        public IEntityQueryBuilder<TEntity> Where(string whereClause, object parameters) {
            this.query.WhereClause = whereClause;
            if (parameters != null) {
                this.query.WhereClauseParameters = parameters.ToDictionary();
            }

            return this;
        }

        public IEntityQueryBuilder<TEntity> OrderBy(string orderByClause) {
            // TODO multiple invocations?
            this.query.OrderByClause = orderByClause;
            return this;
        }

        public IEntityQueryBuilder<TEntity> Offset(int offset) {
            this.query.Offset = offset;
            return this;
        }

        public IEntityQueryBuilder<TEntity> Limit(int limit) {
            this.query.Limit = limit;
            return this;
        }

        public IEntityQueryBuilder<TEntity> Cache(TimeSpan? absoluteExpirationRelativeToNow = null, string key = null) {
            this.query.CacheQuery = true;
            this.query.AbsoluteExpirationRelativeToNow = absoluteExpirationRelativeToNow;
            this.query.CacheKey = key;
            return this;
        }

        public IEntityQueryBuilder<TEntity> NoCache() {
            this.query.CacheQuery = false;
            return this;
        }

        public IFutureEntityQueryResult<TEntity> Future() {
            var queryEngine = this.session.GetEngine();
            queryEngine.Add(this.query);
            return new FutureEntityQueryResult<TEntity>(this.query, this.session);
        }
    }
}