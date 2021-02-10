﻿namespace Leap.Data {
    using System.Threading;
    using System.Threading.Tasks;

    using Fasterflect;

    using Leap.Data.IdentityMap;
    using Leap.Data.Internal;
    using Leap.Data.Internal.Caching;
    using Leap.Data.Schema;
    using Leap.Data.Serialization;

    class Session : ISession {
        private readonly ISchema schema;

        private readonly IMemoryCache memoryCache;

        private readonly IDistributedCache distributedCache;

        private readonly UnitOfWork.UnitOfWork unitOfWork;

        private readonly IdentityMap.IdentityMap identityMap;

        private readonly QueryEngine queryEngine;

        private readonly UpdateEngine updateEngine;

        public Session(
            ISchema schema,
            ISerializer serializer,
            IQueryExecutor queryExecutor,
            IUpdateExecutor updateExecutor,
            IMemoryCache memoryCache,
            IDistributedCache distributedCache) {
            this.schema           = schema;
            this.memoryCache      = memoryCache;
            this.distributedCache = distributedCache;
            this.identityMap      = new IdentityMap.IdentityMap();
            this.unitOfWork   = new UnitOfWork.UnitOfWork(serializer, schema);
            this.queryEngine = new QueryEngine(
                schema,
                this.identityMap,
                this.unitOfWork,
                queryExecutor,
                serializer,
                memoryCache != null ? new MemoryCacheExecutor(memoryCache) : null,
                distributedCache != null ? new DistributedCacheExecutor(distributedCache) : null);
            this.updateEngine = new UpdateEngine(updateExecutor, memoryCache, distributedCache, schema, serializer);
        }

        public IQueryBuilder<TEntity> Get<TEntity>()
            where TEntity : class {
            return new QueryBuilder<TEntity>(this, this.schema.GetDefaultCollection<TEntity>());
        }

        public IQueryBuilder<TEntity> Get<TEntity>(string collectionName)
            where TEntity : class {
            return new QueryBuilder<TEntity>(this, this.schema.GetCollection(collectionName));
        }

        public async Task SaveChangesAsync(CancellationToken cancellationToken = default) {
            // flush the queryEngine
            await this.queryEngine.EnsureExecutedAsync();

            // execute against caches and persistence
            await this.updateEngine.ExecuteAsync(this.unitOfWork, cancellationToken);

            // instantiate new unit of work
            this.unitOfWork.SetPersisted();
        }

        public void Delete<TEntity>(TEntity entity)
            where TEntity : class {
            this.Delete(entity, this.schema.GetDefaultCollection<TEntity>());
        }

        public void Delete<TEntity>(TEntity entity, string collectionName)
            where TEntity : class {
            this.Delete(entity, this.schema.GetCollection(collectionName));
        }

        private void Delete<TEntity>(TEntity entity, Collection collection)
            where TEntity : class {
            this.unitOfWork.UpdateState(collection, entity, DocumentState.Deleted);
        }

        public void Add<TEntity>(TEntity entity)
            where TEntity : class {
            this.Add(entity, this.schema.GetDefaultCollection<TEntity>());
        }

        public void Add<TEntity>(TEntity entity, string collectionName)
            where TEntity : class {
            this.Add(entity, this.schema.GetCollection(collectionName));
        }

        private void Add<TEntity>(TEntity entity, Collection collection) {
            this.unitOfWork.AddOrUpdate(collection, entity, null, DocumentState.New);
            var keyType = collection.KeyType;
            var key = collection.KeyExtractor.CallMethod(new[] { typeof(TEntity), keyType }, nameof(IKeyExtractor.Extract), entity);
            this.identityMap.Add(keyType, key, entity);
        }

        public IEntityInspector<TEntity> Inspect<TEntity>(TEntity entity)
            where TEntity : class {
            return new EntityInspector<TEntity>(this.schema, this.unitOfWork, entity);
        }

        public QueryEngine GetEngine() {
            return this.queryEngine;
        }
    }
}