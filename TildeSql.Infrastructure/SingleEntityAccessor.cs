namespace TildeSql.Infrastructure {
    using TildeSql.Model;

    public class SingleEntityAccessor<TEntity, TKey> : ISingleEntityAccessor<TEntity, TKey> {
        private readonly IFutureSingleResult<TEntity, TKey> futureSingleResult;

        private readonly IFutureEntityQueryResult<TEntity> futureEntityQueryResult;

        public SingleEntityAccessor(IFutureSingleResult<TEntity, TKey> futureSingleResult) {
            this.futureSingleResult = futureSingleResult;
        }

        public SingleEntityAccessor(IFutureEntityQueryResult<TEntity> futureEntityQueryResult) {
            this.futureEntityQueryResult = futureEntityQueryResult;
        }

        public ValueTask<TEntity> SingleOrDefaultAsync() {
            if (this.futureSingleResult != null) {
                return this.futureSingleResult.SingleAsync();
            }

            if (this.futureEntityQueryResult != null) {
                return this.futureEntityQueryResult.SingleOrDefaultAsync();
            }

            throw new Exception("How did you get here?");
        }
    }
}