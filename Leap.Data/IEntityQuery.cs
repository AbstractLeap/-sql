namespace Leap.Data {
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;

    public interface IEntityQuery<TEntity> {
        ValueTask<TEntity> ByKeyAsync<TKey>(TKey key, CancellationToken cancellationToken = default);

        IAsyncEnumerable<TEntity> ByKeyAsync<TKey>(params TKey[] keys);
    }
}