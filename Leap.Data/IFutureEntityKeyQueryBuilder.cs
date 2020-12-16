namespace Leap.Data {
    public interface IFutureEntityKeyQueryBuilder<TEntity> {
        IFutureKeyQuery<TEntity, TKey> ByKeyInTheFuture<TKey>(TKey key);

        IFutureKeyQuery<TEntity, TKey> ByKeyInTheFuture<TKey>(params TKey[] keys);
    }
}