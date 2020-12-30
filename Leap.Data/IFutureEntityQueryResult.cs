namespace Leap.Data {
    using System.Collections.Generic;

    public interface IFutureEntityQueryResult<TEntity> : IAsyncEnumerable<TEntity> { }
}