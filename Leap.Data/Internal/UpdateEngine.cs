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

    class UpdateEngine {
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
            var postPersistDocumentUpdates = new List<Action>();
            foreach (var operation in unitOfWork.Operations) {
                if (operation.IsAddOperation()) {
                    // only have an entity, need to generate database row
                    var newDatabaseRow = operation.GetNewDatabaseRow(this.schema, this.columnValueFactoryFactory);
                    inserts.Add(newDatabaseRow);
                    postPersistDocumentUpdates.Add(
                        () => {
                            operation.UpdateDocument(newDatabaseRow, DocumentState.Persisted);
                        });
                } else if (operation.IsUpdateOperation()) {
                    // need to provide existing row and new row (e.g. to support optimistic concurrency
                    var newDatabaseRow = operation.GetNewDatabaseRow(this.schema, this.columnValueFactoryFactory);
                    updates.Add((operation.GetCurrentDatabaseRow(), newDatabaseRow));
                    postPersistDocumentUpdates.Add(
                        () => {
                            operation.UpdateDocument(newDatabaseRow, DocumentState.Persisted);
                        });
                }
                else {
                    // don't have a new row so just the old one
                    deletes.Add(operation.GetCurrentDatabaseRow());
                }
            }
            
            await this.persistenceUpdateExecutor.ExecuteAsync(inserts, updates, deletes, cancellationToken);
            
            // execute the document updates
            foreach (var postPersistDocumentUpdate in postPersistDocumentUpdates) {
                postPersistDocumentUpdate();
            }
            
            // now we update the caches for additions and updates now
            // if the above fails we won't get here which is good
            foreach (var operation in unitOfWork.Operations) {
                if (operation.IsAddOperation() || operation.IsUpdateOperation()) {
                    // TODO update document in identity map
                    this.CallMethod(operation.GetType().GenericTypeArguments, nameof(UpdateMemoryCache), operation);
                    await (ValueTask)this.CallMethod(operation.GetType().GenericTypeArguments, nameof(UpdateDistributedCacheAsync), operation, cancellationToken);
                }
            }
        }

        #region DeleteCacheMethods

        private ValueTask DeleteFromDistributedCacheAsync<TEntity>(DeleteOperation<TEntity> deleteOperation, CancellationToken cancellationToken) {
            if (this.distributedCache == null) {
                return ValueTask.CompletedTask;
            }

            var table = this.schema.GetTable<TEntity>();
            return (ValueTask)this.CallMethod(new[] { typeof(TEntity), table.KeyType }, nameof(this.DeleteFromDistributedCacheAsync), deleteOperation, table);
        }

        private ValueTask DeleteFromDistributedCacheAsync<TEntity, TKey>(DeleteOperation<TEntity> deleteOperation, Table table, CancellationToken cancellationToken) {
            var key = table.KeyExtractor.Extract<TEntity, TKey>(deleteOperation.Document.Entity);
            return this.distributedCache.RemoveAsync(key, cancellationToken);
        }

        private void DeleteFromMemoryCache<TEntity>(DeleteOperation<TEntity> deleteOperation) {
            if (this.memoryCache == null) {
                return;
            }

            var table = this.schema.GetTable<TEntity>();
            this.CallMethod(new[] { typeof(TEntity), table.KeyType }, nameof(DeleteFromMemoryCache), deleteOperation, table);
        }

        private void DeleteFromMemoryCache<TEntity, TKey>(DeleteOperation<TEntity> deleteOperation, Table table) {
            var key = table.KeyExtractor.Extract<TEntity, TKey>(deleteOperation.Document.Entity);
            this.memoryCache.Remove(key);
        }

        #endregion

        #region UpdateCacheMethods

        private void UpdateMemoryCache<TEntity>(IOperation<TEntity> operation) {
            if (this.memoryCache == null) {
                return;
            }

            var table = this.schema.GetTable<TEntity>();
            this.CallMethod(new[] { typeof(TEntity), table.KeyType }, nameof(UpdateMemoryCache), operation, table);
        }

        private void UpdateMemoryCache<TEntity, TKey>(IOperation<TEntity> operation, Table table) {
            var key = table.KeyExtractor.Extract<TEntity, TKey>(operation.Document.Entity);
            this.memoryCache.Set(key, operation.Document.Row.Values);
        }

        private ValueTask UpdateDistributedCacheAsync<TEntity>(IOperation<TEntity> operation, CancellationToken cancellationToken) {
            if (this.distributedCache == null) {
                return ValueTask.CompletedTask;
            }

            var table = this.schema.GetTable<TEntity>();
            return (ValueTask)this.CallMethod(new[] { typeof(TEntity), table.KeyType }, nameof(UpdateDistributedCacheAsync), operation, table, cancellationToken);
        }

        private ValueTask UpdateDistributedCacheAsync<TEntity, TKey>(IOperation<TEntity> operation, Table table, CancellationToken cancellationToken) {
            var key = table.KeyExtractor.Extract<TEntity, TKey>(operation.Document.Entity);
            return this.distributedCache.SetAsync(key, operation.Document.Row.Values, cancellationToken);
        }

        #endregion
    }
}