namespace Leap.Data {
    using System.Collections.Generic;
    using System.Threading;

    using Leap.Data.Queries;

    class FutureEntityQueryResult<TEntity> : IFutureEntityQueryResult<TEntity>
        where TEntity : class {
        private readonly EntityQuery<TEntity> query;

        private readonly Session session;

        public FutureEntityQueryResult(EntityQuery<TEntity> query, Session session) {
            this.query   = query;
            this.session = session;
        }

        public IAsyncEnumerator<TEntity> GetAsyncEnumerator(CancellationToken cancellationToken = new CancellationToken()) {
            return this.session.GetEngine().GetResult<TEntity>(this.query).GetAsyncEnumerator(cancellationToken);
        }
    }
}