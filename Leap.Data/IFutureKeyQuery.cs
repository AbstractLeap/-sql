namespace Leap.Data {
    using System.Collections.Generic;

    public interface IFutureKeyQuery<TEntity, TKey> : IAsyncEnumerable<TEntity> { }
}