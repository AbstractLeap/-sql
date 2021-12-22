namespace TildeSql {
    using System.Collections.Generic;
    using System.Threading;

    using TildeSql.Queries;

    internal class FutureMultipleResult<TEntity, TKey> : IFutureMultipleResult<TEntity, TKey>
        where TEntity : class {
        private readonly MultipleKeyQuery<TEntity, TKey> query;

        private readonly Session session;

        public FutureMultipleResult(MultipleKeyQuery<TEntity, TKey> query, Session session) {
            this.query   = query;
            this.session = session;
        }

        public IAsyncEnumerator<TEntity> GetAsyncEnumerator(CancellationToken cancellationToken = new CancellationToken()) {
            return this.session.GetEngine().GetResult<TEntity>(this.query).GetAsyncEnumerator(cancellationToken);
        }
    }
}