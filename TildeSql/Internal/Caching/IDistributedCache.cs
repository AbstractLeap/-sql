namespace TildeSql.Internal.Caching {
    using System.Threading;
    using System.Threading.Tasks;

    public interface IDistributedCache {
        ValueTask SetAsync<TItem>(string key, TItem value, CancellationToken cancellationToken);

        ValueTask<TItem> GetAsync<TItem>(string key, CancellationToken cancellationToken);

        ValueTask RemoveAsync(string key, CancellationToken cancellationToken);
    }
}