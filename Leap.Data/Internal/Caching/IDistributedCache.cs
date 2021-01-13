namespace Leap.Data.Internal.Caching {
    using System.Threading;
    using System.Threading.Tasks;

    public interface IDistributedCache {
        ValueTask SetAsync<TItem>(string key, TItem value, CancellationToken cancellationToken);

        ValueTask<TItem> GetAsync<TItem>(object key, CancellationToken cancellationToken);

        ValueTask RemoveAsync(object key, CancellationToken cancellationToken);
    }
}