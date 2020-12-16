namespace Leap.Data {
    using System.Collections.Generic;

    public interface IFutureEntityQuery<TEntity> : IAsyncEnumerable<TEntity> { }
}