namespace TildeSql {
    using System.Collections.Generic;

    public interface IFutureEntityKeyQueryBuilder<TEntity> {
        IFutureSingleResult<TEntity, TKey> SingleFuture<TKey>(TKey key);

        IFutureMultipleResult<TEntity, TKey> MultipleFuture<TKey>(IEnumerable<TKey> keys);
    }
}