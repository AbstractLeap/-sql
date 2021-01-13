namespace Leap.Data.Internal.Caching {
    public interface IMemoryCache {
        void Set<TItem>(object key, TItem value);

        bool TryGetValue<TItem>(object key, out TItem item);

        void Remove(object key);
    }
}