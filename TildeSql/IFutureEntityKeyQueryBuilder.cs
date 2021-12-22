namespace TildeSql {
    public interface IFutureEntityKeyQueryBuilder<TEntity> {
        IFutureSingleResult<TEntity, TKey> SingleFuture<TKey>(TKey key);

        IFutureMultipleResult<TEntity, TKey> MultipleFuture<TKey>(params TKey[] keys);
    }
}