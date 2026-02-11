namespace TildeSql.Configuration {
    using System;

    using Microsoft.Extensions.Caching.Distributed;
    using Microsoft.Extensions.Caching.Memory;

    using TildeSql.Events;
    using TildeSql.Internal;
    using TildeSql.Internal.Caching;
    using TildeSql.Schema;
    using TildeSql.Serialization;

    public class Configuration {
        public Configuration(ISchema schema) {
            this.Schema = schema;
        }

        public ISchema Schema { get; }

        public ISerializer Serializer { get; set; } = new SystemJsonSerializer();

        public IChangeDetector ChangeDetector { get; set; }

        public Func<IPersistenceQueryExecutor> QueryExecutorFactory { get; set; }

        public Func<IUpdateExecutor> UpdateExecutorFactory { get; set; }
        
        public IMemoryCache MemoryCache { get; set; }
        
        public IDistributedCache DistributedCache { get; set; }

        public ICacheSerializer CacheSerializer { get; set; }

        public CacheOptions CacheOptions { get; set; }

        public ISaveChangesEventListener SaveChangesEventListener { get; set; }

        public ISessionFactory BuildSessionFactory() {
            return new SessionFactory(
                this.Schema,
                this.Serializer,
                this.ChangeDetector,
                this.QueryExecutorFactory,
                this.UpdateExecutorFactory,
                this.MemoryCache,
                this.DistributedCache,
                this.CacheSerializer,
                this.CacheOptions,
                this.SaveChangesEventListener);
        }
    }
}