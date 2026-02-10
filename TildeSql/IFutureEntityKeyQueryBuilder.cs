namespace TildeSql {
    using System.Collections.Generic;

    public interface IFutureEntityKeyQueryBuilder<TEntity> {
        IFutureSingleResult<TEntity, TKey> SingleFuture<TKey>(TKey key, bool disableCache = false, bool disableTracking = false);

        IFutureMultipleResult<TEntity, TKey> MultipleFuture<TKey>(IEnumerable<TKey> keys, bool disableCache = false, bool disableTracking = false);
    }
}