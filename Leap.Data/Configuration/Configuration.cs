namespace Leap.Data.Configuration {
    using System;

    using Leap.Data.Events;
    using Leap.Data.Internal;
    using Leap.Data.Internal.Caching;
    using Leap.Data.Schema;
    using Leap.Data.Serialization;

    public class Configuration {
        public Configuration(ISchema schema) {
            this.Schema = schema;
        }

        public ISchema Schema { get; }

        public ISerializer Serializer { get; set; } = new SystemJsonSerializer();

        public Func<IQueryExecutor> QueryExecutorFactory { get; set; }

        public Func<IUpdateExecutor> UpdateExecutorFactory { get; set; }
        
        public IMemoryCache MemoryCache { get; set; }
        
        public IDistributedCache DistributedCache { get; set; }

        public ISaveChangesEventListener SaveChangesEventListener { get; set; }

        public ISessionFactory BuildSessionFactory() {
            return new SessionFactory(
                this.Schema,
                this.Serializer,
                this.QueryExecutorFactory,
                this.UpdateExecutorFactory,
                this.MemoryCache,
                this.DistributedCache,
                this.SaveChangesEventListener);
        }
    }
}