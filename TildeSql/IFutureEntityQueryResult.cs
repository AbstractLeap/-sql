namespace TildeSql {
    using System.Collections.Generic;

    public interface IFutureEntityQueryResult<TEntity> : IAsyncEnumerable<TEntity> { }
}