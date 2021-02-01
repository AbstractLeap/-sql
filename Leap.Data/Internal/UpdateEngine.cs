namespace Leap.Data.Internal {
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;

    using Fasterflect;

    using Leap.Data.IdentityMap;
    using Leap.Data.Internal.Caching;
    using Leap.Data.Internal.ColumnValueFactories;
    using Leap.Data.Operations;
    using Leap.Data.Schema;
    using Leap.Data.Serialization;
    using Leap.Data.UnitOfWork;

    class UpdateEngine : IAsyncDisposable {
        private readonly IUpdateExecutor persistenceUpdateExecutor;

        private readonly IMemoryCache memoryCache;

        private readonly IDistributedCache distributedCache;

        private readonly ISchema schema;

        private readonly ColumnValueFactoryFactory columnValueFactoryFactory;

        public UpdateEngine(IUpdateExecutor persistenceUpdateExecutor, IMemoryCache memoryCache, IDistributedCache distributedCache, ISchema schema, ISerializer serializer) {
            this.persistenceUpdateExecutor = persistenceUpdateExecutor;
            this.memoryCache               = memoryCache;
            this.distributedCache          = distributedCache;
            this.schema                    = schema;
            this.columnValueFactoryFactory        = new ColumnValueFactoryFactory(serializer);
        }

        public async ValueTask ExecuteAsync(UnitOfWork unitOfWork, CancellationToken cancellationToken = default) {
            if (!unitOfWork.Operations.Any()) {
                return;
            }

            // we delete from cache now, as failure on the persistence is ok
            foreach (var operation in unitOfWork.Operations) {
                if (operation.IsDeleteOperation()) {
                    this.CallMethod(operation.GetType().GenericTypeArguments, nameof(DeleteFromMemoryCache), operation);
                    await (ValueTask)this.CallMethod(operation.GetType().GenericTypeArguments, nameof(this.DeleteFromDistributedCacheAsync), operation, cancellationToken);
                }
            }

            var inserts = new List<DatabaseRow>(); // new database rows
            var updates = new List<(DatabaseRow OldDatabaseRow, DatabaseRow NewDatabaseRow)>(); // both the old row and the new row
            var deletes = new List<DatabaseRow>(); // just the old row
            var postPersistDocumentUpdates = new List<Func<ValueTask>>();
            foreach (var operation in unitOfWork.Operations) {
                if (operation.IsAddOperation()) {
                    // only have an entity, need to generate database row
                    var newDatabaseRow = operation.GetNewDatabaseRow(this.schema, this.columnValueFactoryFactory);
                    inserts.Add(newDatabaseRow);
                    postPersistDocumentUpdates.Add(
                        async () => {
                            unitOfWork.UpdateRow(operation.GetEntity().GetType(), operation.Table, operation.GetEntity(), newDatabaseRow);
                            this.CallMethod(operation.GetType().GenericTypeArguments, nameof(UpdateMemoryCache), operation.GetEntity(), newDatabaseRow);
                            await (ValueTask)this.CallMethod(operation.GetType().GenericTypeArguments, nameof(UpdateDistributedCacheAsync), operation.GetEntity(), newDatabaseRow, cancellationToken);
                        });
                } else if (operation.IsUpdateOperation()) {
                    // need to provide existing row and new row (e.g. to support optimistic concurrency
                    var newDatabaseRow = operation.GetNewDatabaseRow(this.schema, this.columnValueFactoryFactory);
                    updates.Add((unitOfWork.GetRow(operation.GetEntity().GetType(), operation.Table, operation.GetEntity()), newDatabaseRow));
                    postPersistDocumentUpdates.Add(
                        async () => {
                            unitOfWork.UpdateRow(operation.GetEntity().GetType(), operation.Table, operation.GetEntity(), newDatabaseRow);
                            this.CallMethod(operation.GetType().GenericTypeArguments, nameof(UpdateMemoryCache), operation.GetEntity(), newDatabaseRow);
                            await (ValueTask)this.CallMethod(operation.GetType().GenericTypeArguments, nameof(UpdateDistributedCacheAsync), operation.GetEntity(), newDatabaseRow, cancellationToken);
                        });
                }
                else {
                    // don't have a new row so just the old one
                    deletes.Add(unitOfWork.GetRow(operation.GetEntity().GetType(), operation.Table, operation.GetEntity()));
                }
            }
            
            await this.persistenceUpdateExecutor.ExecuteAsync(inserts, updates, deletes, cancellationToken);
            
            // execute the document updates
            foreach (var postPersistDocumentUpdate in postPersistDocumentUpdates) {
                await postPersistDocumentUpdate();
            }
        }

        #region DeleteCacheMethods

        private ValueTask DeleteFromDistributedCacheAsync<TEntity>(DeleteOperation<TEntity> deleteOperation, CancellationToken cancellationToken) {
            if (this.distributedCache == null) {
                return ValueTask.CompletedTask;
            }
            
            return (ValueTask)this.CallMethod(new[] { typeof(TEntity), deleteOperation.Table.KeyType }, nameof(this.DeleteFromDistributedCacheAsync), deleteOperation, deleteOperation.Table);
        }

        private ValueTask DeleteFromDistributedCacheAsync<TEntity, TKey>(DeleteOperation<TEntity> deleteOperation, Table table, CancellationToken cancellationToken) {
            var key = table.KeyExtractor.Extract<TEntity, TKey>(deleteOperation.Entity);
            return this.distributedCache.RemoveAsync(key, cancellationToken);
        }

        private void DeleteFromMemoryCache<TEntity>(DeleteOperation<TEntity> deleteOperation) {
            if (this.memoryCache == null) {
                return;
            }
            
            this.CallMethod(new[] { typeof(TEntity), deleteOperation.Table.KeyType }, nameof(DeleteFromMemoryCache), deleteOperation, deleteOperation.Table);
        }

        private void DeleteFromMemoryCache<TEntity, TKey>(DeleteOperation<TEntity> deleteOperation, Table table) {
            var key = table.KeyExtractor.Extract<TEntity, TKey>(deleteOperation.Entity);
            this.memoryCache.Remove(key);
        }

        #endregion

        #region UpdateCacheMethods

        private void UpdateMemoryCache<TEntity>(TEntity entity, DatabaseRow row) {
            if (this.memoryCache == null) {
                return;
            }
            
            this.CallMethod(new[] { typeof(TEntity), row.Table.KeyType }, nameof(UpdateMemoryCache), entity, row, row.Table);
        }

        private void UpdateMemoryCache<TEntity, TKey>(TEntity entity, DatabaseRow row, Table table) {
            var key = table.KeyExtractor.Extract<TEntity, TKey>(entity);
            this.memoryCache.Set(key, row.Values);
        }

        private ValueTask UpdateDistributedCacheAsync<TEntity>(TEntity entity, DatabaseRow row, CancellationToken cancellationToken) {
            if (this.distributedCache == null) {
                return ValueTask.CompletedTask;
            }
            
            return (ValueTask)this.CallMethod(new[] { typeof(TEntity), row.Table.KeyType }, nameof(UpdateDistributedCacheAsync), entity, row, row.Table, cancellationToken);
        }

        private ValueTask UpdateDistributedCacheAsync<TEntity, TKey>(TEntity entity, DatabaseRow row, Table table, CancellationToken cancellationToken) {
            var key = table.KeyExtractor.Extract<TEntity, TKey>(entity);
            return this.distributedCache.SetAsync(key, row.Values, cancellationToken);
        }

        #endregion

        public async ValueTask DisposeAsync() {
            if (this.persistenceUpdateExecutor is IAsyncDisposable disposable) {
                await disposable.DisposeAsync();
            }
        }
    }
}