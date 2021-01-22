﻿namespace Leap.Data.MemoryCache {
    using Microsoft.Extensions.Caching.Memory;
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Options;

    using IMemoryCache = Leap.Data.Internal.Caching.IMemoryCache;

    public class MemoryCacheAdapter : IMemoryCache {
        private readonly MemoryCache memoryCache;

        public MemoryCacheAdapter(IOptions<MemoryCacheOptions> optionsAccessor, ILoggerFactory loggerFactory) {
            this.memoryCache = new MemoryCache(optionsAccessor, loggerFactory);
        }

        public void Set<TItem>(object key, TItem value) {
            this.memoryCache.Set(key, value);
        }

        public bool TryGetValue<TItem>(object key, out TItem item) {
            return this.memoryCache.TryGetValue(key, out item);
        }

        public void Remove(object key) {
            this.memoryCache.Remove(key);
        }
    }
}