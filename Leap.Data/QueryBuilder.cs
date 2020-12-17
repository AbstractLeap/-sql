namespace Leap.Data {
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;

    using Leap.Data.Queries;

    internal class QueryBuilder<TEntity> : IQueryBuilder<TEntity>
        where TEntity : class {
        private readonly Session session;

        public QueryBuilder(Session session) {
            this.session = session;
        }
        
        public ValueTask<TEntity> SingleAsync<TKey>(TKey key, CancellationToken cancellationToken = default) {
            var query = new KeyQuery<TEntity, TKey>(key);
            var queryEngine = this.session.GetEngine();
            return queryEngine.GetResult<TEntity>(query).SingleOrDefaultAsync(cancellationToken);
        }
        
        public IFutureSingleResult<TEntity, TKey> SingleFuture<TKey>(TKey key) {
            var query = new KeyQuery<TEntity, TKey>(key);
            var queryEngine = this.session.GetEngine();
            queryEngine.Add(query);
            return new FutureSingleResult<TEntity, TKey>(query, this.session);
        }

        public IAsyncEnumerable<TEntity> MultipleAsync<TKey>(params TKey[] keys) {
            throw new NotImplementedException();
        }

        public IFutureMultipleResult<TEntity, TKey> MultipleFuture<TKey>(params TKey[] keys) {
            throw new NotImplementedException();
        }

        public IAsyncEnumerator<TEntity> GetAsyncEnumerator(CancellationToken cancellationToken = new CancellationToken()) {
            throw new NotImplementedException();
        }

        public IEntityQueryBuilder<TEntity> Where(string whereClause) {
            throw new NotImplementedException();
        }

        public IEntityQueryBuilder<TEntity> OrderBy(string orderByClause) {
            throw new NotImplementedException();
        }

        public IEntityQueryBuilder<TEntity> Offset(int offset) {
            throw new NotImplementedException();
        }

        public IEntityQueryBuilder<TEntity> Limit(int limit) {
            throw new NotImplementedException();
        }

        public IFutureEntityQuery<TEntity> Future() {
            throw new NotImplementedException();
        }
    }
}