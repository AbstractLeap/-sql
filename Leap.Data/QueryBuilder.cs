namespace Leap.Data {
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;

    internal class QueryBuilder<TEntity> : IQueryBuilder<TEntity> {
        public Task<TEntity> ByKeyAsync<TKey>(TKey key, CancellationToken cancellationToken = default) {
            throw new NotImplementedException();
        }

        public IAsyncEnumerable<TEntity> ByKeyAsync<TKey>(params TKey[] keys) {
            throw new NotImplementedException();
        }

        public IFutureKeyQuery<TEntity, TKey> ByKeyInTheFuture<TKey>(TKey key) {
            throw new NotImplementedException();
        }

        public IFutureKeyQuery<TEntity, TKey> ByKeyInTheFuture<TKey>(params TKey[] keys) {
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

        public IFutureEntityQuery<TEntity> InTheFuture() {
            throw new NotImplementedException();
        }
    }
}