namespace Leap.Data {
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;

    using Leap.Data.Queries;
    using Leap.Data.Schema;

    internal class QueryBuilder<TEntity> : IQueryBuilder<TEntity>
        where TEntity : class {
        private readonly Session session;

        private readonly Table table;

        public QueryBuilder(Session session, Table table) {
            this.session = session;
            this.table   = table;
        }

        public ValueTask<TEntity> SingleAsync<TKey>(TKey key, CancellationToken cancellationToken = default) {
            var query = new KeyQuery<TEntity, TKey>(key, this.table);
            var queryEngine = this.session.GetEngine();
            return queryEngine.GetResult<TEntity>(query).SingleOrDefaultAsync(cancellationToken);
        }

        public IFutureSingleResult<TEntity, TKey> SingleFuture<TKey>(TKey key) {
            var query = new KeyQuery<TEntity, TKey>(key, this.table);
            var queryEngine = this.session.GetEngine();
            queryEngine.Add(query);
            return new FutureSingleResult<TEntity, TKey>(query, this.session);
        }

        public IAsyncEnumerable<TEntity> MultipleAsync<TKey>(params TKey[] keys) {
            var query = new MultipleKeyQuery<TEntity, TKey>(keys, this.table);
            var queryEngine = this.session.GetEngine();
            return queryEngine.GetResult<TEntity>(query);
        }

        public IFutureMultipleResult<TEntity, TKey> MultipleFuture<TKey>(params TKey[] keys) {
            var query = new MultipleKeyQuery<TEntity, TKey>(keys, this.table);
            var queryEngine = this.session.GetEngine();
            queryEngine.Add(query);
            return new FutureMultipleResult<TEntity, TKey>(query, this.session);
        }

        public IAsyncEnumerator<TEntity> GetAsyncEnumerator(CancellationToken cancellationToken = new CancellationToken()) {
            var query = new EntityQuery<TEntity>(this.table);
            var queryEngine = this.session.GetEngine();
            return queryEngine.GetResult<TEntity>(query).GetAsyncEnumerator(cancellationToken);
        }

        public IEntityQueryBuilder<TEntity> Where(string whereClause) {
            var entityQueryBuilder = new EntityQueryBuilder<TEntity>(this.session, this.table);
            entityQueryBuilder.Where(whereClause);
            return entityQueryBuilder;
        }

        public IEntityQueryBuilder<TEntity> OrderBy(string orderByClause) {
            var entityQueryBuilder = new EntityQueryBuilder<TEntity>(this.session, this.table);
            entityQueryBuilder.OrderBy(orderByClause);
            return entityQueryBuilder;
        }

        public IEntityQueryBuilder<TEntity> Offset(int offset) {
            var entityQueryBuilder = new EntityQueryBuilder<TEntity>(this.session, this.table);
            entityQueryBuilder.Offset(offset);
            return entityQueryBuilder;
        }

        public IEntityQueryBuilder<TEntity> Limit(int limit) {
            var entityQueryBuilder = new EntityQueryBuilder<TEntity>(this.session, this.table);
            entityQueryBuilder.Limit(limit);
            return entityQueryBuilder;
        }

        public IFutureEntityQueryResult<TEntity> Future() {
            var query = new EntityQuery<TEntity>(this.table);
            var queryEngine = this.session.GetEngine();
            queryEngine.Add(query);
            return new FutureEntityQueryResult<TEntity>(query, this.session);
        }
    }
}