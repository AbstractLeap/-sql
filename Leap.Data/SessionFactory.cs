﻿namespace Leap.Data {
    using System;

    using Leap.Data.Events;
    using Leap.Data.Internal;
    using Leap.Data.Internal.Caching;
    using Leap.Data.Schema;
    using Leap.Data.Serialization;

    class SessionFactory : ISessionFactory {
        private readonly ISchema schema;

        private readonly ISerializer serializer;

        private readonly Func<IQueryExecutor> queryExecutorFactory;

        private readonly Func<IUpdateExecutor> updateExecutorFactory;

        private readonly IMemoryCache memoryCache;

        private readonly IDistributedCache distributedCache;

        private readonly ISaveChangesEventListener saveChangesEventListener;

        public SessionFactory(
            ISchema schema,
            ISerializer serializer,
            Func<IQueryExecutor> queryExecutorFactory,
            Func<IUpdateExecutor> updateExecutorFactory,
            IMemoryCache memoryCache,
            IDistributedCache distributedCache,
            ISaveChangesEventListener saveChangesEventListener) {
            this.schema                   = schema;
            this.serializer               = serializer;
            this.queryExecutorFactory     = queryExecutorFactory;
            this.updateExecutorFactory    = updateExecutorFactory;
            this.memoryCache              = memoryCache;
            this.distributedCache         = distributedCache;
            this.saveChangesEventListener = saveChangesEventListener;
        }

        public ISession StartSession() {
            return new Session(
                this.schema,
                this.serializer,
                this.queryExecutorFactory(),
                this.updateExecutorFactory(),
                this.memoryCache,
                this.distributedCache,
                this.saveChangesEventListener);
        }
    }
}