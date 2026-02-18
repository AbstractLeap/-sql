namespace TildeSql {
    using System;

    using Microsoft.Extensions.Caching.Distributed;
    using Microsoft.Extensions.Caching.Memory;

    using TildeSql.Events;
    using TildeSql.Internal;
    using TildeSql.Internal.Caching;
    using TildeSql.Schema;
    using TildeSql.Serialization;

    class SessionFactory : ISessionFactory {
        private readonly ISchema schema;

        private readonly ISerializer serializer;

        private readonly IChangeDetector changeDetector;

        private readonly Func<IPersistenceQueryExecutor> queryExecutorFactory;

        private readonly Func<IUpdateExecutor> updateExecutorFactory;

        private readonly IMemoryCache memoryCache;

        private readonly IDistributedCache distributedCache;

        private readonly ICacheSerializer cacheSerializer;

        private readonly CacheOptions cacheOptions;

        private readonly ISaveChangesEventListener saveChangesEventListener;

        public SessionFactory(
            ISchema schema,
            ISerializer serializer,
            IChangeDetector changeDetector,
            Func<IPersistenceQueryExecutor> queryExecutorFactory,
            Func<IUpdateExecutor> updateExecutorFactory,
            IMemoryCache memoryCache,
            IDistributedCache distributedCache,
            ICacheSerializer cacheSerializer,
            CacheOptions cacheOptions,
            ISaveChangesEventListener saveChangesEventListener) {
            this.schema                   = schema;
            this.serializer               = serializer;
            this.changeDetector           = changeDetector;
            this.queryExecutorFactory     = queryExecutorFactory;
            this.updateExecutorFactory    = updateExecutorFactory;
            this.memoryCache              = memoryCache;
            this.distributedCache         = distributedCache;
            this.cacheSerializer          = cacheSerializer;
            this.cacheOptions             = cacheOptions;
            this.saveChangesEventListener = saveChangesEventListener;
        }

        public ISession StartSession() {
            return new Session(
                this.schema,
                this.serializer,
                this.changeDetector,
                this.queryExecutorFactory(),
                this.updateExecutorFactory(),
                this.memoryCache,
                this.distributedCache,
                this.saveChangesEventListener,
                this.cacheSerializer,
                this.cacheOptions);
        }
    }
}