namespace Leap.Data {
    using System.Linq;
    using System.Threading.Tasks;

    using Leap.Data.Queries;

    internal class FutureSingleResult<TEntity, TKey> : IFutureSingleResult<TEntity, TKey>
        where TEntity : class {
        private readonly KeyQuery<TEntity, TKey> query;

        private readonly Session session;

        public FutureSingleResult(KeyQuery<TEntity, TKey> query, Session session) {
            this.query   = query;
            this.session = session;
        }

        public ValueTask<TEntity> SingleAsync() {
            return this.session.GetEngine().GetResult<TEntity>(this.query).SingleOrDefaultAsync();
        }
    }
}