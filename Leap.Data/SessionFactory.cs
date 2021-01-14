namespace Leap.Data {
    using Leap.Data.Internal;
    using Leap.Data.Internal.Caching;
    using Leap.Data.Schema;
    using Leap.Data.Serialization;

    class SessionFactory : ISessionFactory {
        private readonly ISchema schema;

        private readonly ISerializer serializer;

        private readonly IQueryExecutor queryExecutor;

        private readonly IUpdateExecutor updateExecutor;

        private readonly IMemoryCache memoryCache;

        private readonly IDistributedCache distributedCache;

        public SessionFactory(ISchema schema, ISerializer serializer, IQueryExecutor queryExecutor, IUpdateExecutor updateExecutor, IMemoryCache memoryCache, IDistributedCache distributedCache) {
            this.schema                   = schema;
            this.serializer               = serializer;
            this.queryExecutor            = queryExecutor;
            this.updateExecutor           = updateExecutor;
            this.memoryCache              = memoryCache;
            this.distributedCache         = distributedCache;
        }

        public ISession StartSession() {
            return new Session(this.schema, this.serializer, this.queryExecutor, this.updateExecutor, this.memoryCache, this.distributedCache);
        }
    }
}