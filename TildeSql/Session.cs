﻿namespace TildeSql {
    using System;
    using System.Threading;
    using System.Threading.Tasks;

    using Fasterflect;

    using TildeSql.Events;
    using TildeSql.IdentityMap;
    using TildeSql.Internal;
    using TildeSql.Internal.Caching;
    using TildeSql.Schema;
    using TildeSql.Serialization;

    sealed class Session : ISession {
        private readonly ISchema schema;

        private readonly ISerializer serializer;

        private readonly IMemoryCache memoryCache;

        private readonly IDistributedCache distributedCache;

        private readonly ISaveChangesEventListener saveChangesEventListener;

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
            IDistributedCache distributedCache,
            ISaveChangesEventListener saveChangesEventListener) {
            this.schema                   = schema;
            this.serializer               = serializer;
            this.memoryCache              = memoryCache;
            this.distributedCache         = distributedCache;
            this.saveChangesEventListener = saveChangesEventListener;
            this.identityMap              = new IdentityMap.IdentityMap();
            this.unitOfWork               = new UnitOfWork.UnitOfWork(serializer, schema);
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
            return new QueryBuilder<TEntity>(this, this.schema.GetCollection<TEntity>(collectionName));
        }

        public async Task SaveChangesAsync(CancellationToken cancellationToken = default) {
            if (this.saveChangesEventListener != null) {
                await this.saveChangesEventListener.OnBeforeSaveChangesAsync(this);
            }

            // flush the queryEngine
            await this.queryEngine.EnsureCleanAsync(cancellationToken);

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
            this.Delete(entity, this.schema.GetCollection<TEntity>(collectionName));
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
            this.Add(entity, this.schema.GetCollection<TEntity>(collectionName));
        }

        private void Add<TEntity>(TEntity entity, Collection collection) {
            this.unitOfWork.AddOrUpdate(collection, entity, null, DocumentState.New);
            var keyType = collection.KeyType;
            var key = collection.CallMethod(new[] { typeof(TEntity), keyType }, nameof(Collection.GetKey), entity);
            this.identityMap.Add(keyType, key, entity);
        }

        public IEntityInspector<TEntity> Inspect<TEntity>(TEntity entity)
            where TEntity : class {
            return new EntityInspector<TEntity>(this.schema, this.unitOfWork, entity);
        }

        public IEntityInspector<TEntity> Inspect<TEntity>(TEntity entity, string collectionName)
            where TEntity : class {
            return new EntityInspector<TEntity>(this.schema.GetCollection<TEntity>(collectionName), this.unitOfWork, entity);
        }

        [Obsolete("This is a preview feature")]
        public void Attach<TEntity, TKey>(TEntity entity)
            where TEntity : class {
            this.Attach<TEntity, TKey>(entity, this.schema.GetDefaultCollection<TEntity>());
        }

        [Obsolete("This is a preview feature")]
        public void Attach<TEntity, TKey>(TEntity entity, string collectionName)
            where TEntity : class {
            this.Attach<TEntity, TKey>(entity, this.schema.GetCollection<TEntity>(collectionName));
        }

        private void Attach<TEntity, TKey>(TEntity entity, Collection collection) {
            var rowFactory = new DatabaseRowFactory(this.serializer);
            this.unitOfWork.AddOrUpdate(collection, entity, rowFactory.Create<TEntity, TKey>(collection, entity), DocumentState.Persisted);
        }

        public QueryEngine GetEngine() {
            return this.queryEngine;
        }

        public async ValueTask DisposeAsync() {
            await this.queryEngine.DisposeAsync();
            await this.updateEngine.DisposeAsync();
        }

        public void Dispose() {
            this.queryEngine.Dispose();
            this.updateEngine.Dispose();
        }
    }
}