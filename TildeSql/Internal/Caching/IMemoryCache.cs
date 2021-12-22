namespace TildeSql.Internal.Caching {
    public interface IMemoryCache {
        void Set<TItem>(string key, TItem value);

        bool TryGetValue<TItem>(string key, out TItem item);

        void Remove(string key);
    }
}