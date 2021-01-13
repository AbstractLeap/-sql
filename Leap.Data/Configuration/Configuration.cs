namespace Leap.Data.Configuration {
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

        public IQueryExecutor QueryExecutor { get; set; }

        public IUpdateExecutor UpdateExecutor { get; set; }
        
        public IMemoryCache MemoryCache { get; set; }
        
        public IDistributedCache DistributedCache { get; set; }

        public ISessionFactory BuildSessionFactory() {
            return new SessionFactory(this.Schema, this.Serializer, this.QueryExecutor, this.UpdateExecutor, this.MemoryCache, this.DistributedCache);
        }
    }
}