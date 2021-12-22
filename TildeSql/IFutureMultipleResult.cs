namespace TildeSql {
    using System.Collections.Generic;

    public interface IFutureMultipleResult<TEntity, TKey> : IAsyncEnumerable<TEntity> { }
}