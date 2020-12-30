namespace Leap.Data {
    using System.Linq;
    using System.Threading.Tasks;

    using Leap.Data.Queries;

    internal class FutureMultipleResult<TEntity, TKey> : IFutureMultipleResult<TEntity, TKey>
        where TEntity : class {
        private readonly MultipleKeyQuery<TEntity, TKey> query;

        private readonly Session session;

        public FutureSingleResult(MultipleKeyQuery<TEntity, TKey> query, Session session) {
            this.query   = query;
            this.session = session;
        }
    }
}