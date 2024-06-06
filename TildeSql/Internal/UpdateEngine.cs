namespace TildeSql.Internal {
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;

    using Fasterflect;

    using Microsoft.Extensions.Caching.Distributed;
    using Microsoft.Extensions.Caching.Memory;

    using TildeSql.Internal.Caching;
    using TildeSql.Operations;
    using TildeSql.Schema;
    using TildeSql.Serialization;
    using TildeSql.UnitOfWork;

    class UpdateEngine : IAsyncDisposable, IDisposable {
        private readonly IUpdateExecutor persistenceUpdateExecutor;

        private readonly IMemoryCache memoryCache;

        private readonly IDistributedCache distributedCache;

        private readonly ISchema schema;

        private readonly ISerializer serializer;

        private readonly DatabaseRowFactory databaseRowFactory;

        public UpdateEngine(IUpdateExecutor persistenceUpdateExecutor, IMemoryCache memoryCache, IDistributedCache distributedCache, ISchema schema, ISerializer serializer) {
            this.persistenceUpdateExecutor = persistenceUpdateExecutor;
            this.memoryCache               = memoryCache;
            this.distributedCache          = distributedCache;
            this.schema                    = schema;
            this.serializer                = serializer;
            this.databaseRowFactory        = new DatabaseRowFactory(serializer);
        }

        public async ValueTask ExecuteAsync(UnitOfWork unitOfWork, CancellationToken cancellationToken = default) {
            var operations = unitOfWork.Operations.ToArray();
            if (!operations.Any()) {
                return;
            }

            // we delete from cache now, as failure on the persistence is ok
            foreach (var operation in operations) {
                if (operation.IsDeleteOperation()) {
                    this.CallMethod(operation.GetType().GenericTypeArguments, nameof(DeleteFromMemoryCache), operation);
                    await (ValueTask)this.CallMethod(operation.GetType().GenericTypeArguments, nameof(this.DeleteFromDistributedCacheAsync), operation, cancellationToken);
                }
            }

            var inserts = new List<DatabaseRow>(); // new database rows
            var updates = new List<(DatabaseRow OldDatabaseRow, DatabaseRow NewDatabaseRow)>(); // both the old row and the new row
            var deletes = new List<DatabaseRow>(); // just the old row
            var postPersistDocumentUpdates = new List<Func<ValueTask>>();
            foreach (var operation in operations) {
                if (operation.IsAddOperation()) {
                    // only have an entity, need to generate database row
                    var newDatabaseRow = operation.GetNewDatabaseRow(this.schema, this.databaseRowFactory);
                    inserts.Add(newDatabaseRow);
                    postPersistDocumentUpdates.Add(
                        async () => {
                            if (operation.Collection.IsKeyComputed) {
                                var entity = operation.GetEntity();
                                var computedKey = newDatabaseRow.Values[operation.Collection.GetColumnIndex(operation.Collection.KeyColumns.First().Name)];
                                operation.Collection.CallMethod(new[] { entity.GetType(), operation.Collection.KeyType }, nameof(Collection.SetKey), entity, computedKey);
                                newDatabaseRow.Values[operation.Collection.GetColumnIndex(SpecialColumns.Document)] = this.serializer.Serialize(entity);
                            }

                            unitOfWork.UpdateRow(operation.GetEntity().GetType(), operation.Collection, operation.GetEntity(), newDatabaseRow);
                            this.CallMethod(operation.GetType().GenericTypeArguments, nameof(UpdateMemoryCache), operation.GetEntity(), newDatabaseRow);
                            await (ValueTask)this.CallMethod(
                                operation.GetType().GenericTypeArguments,
                                nameof(UpdateDistributedCacheAsync),
                                operation.GetEntity(),
                                newDatabaseRow,
                                cancellationToken);
                        });
                }
                else if (operation.IsUpdateOperation()) {
                    // need to provide existing row and new row (e.g. to support optimistic concurrency
                    var newDatabaseRow = operation.GetNewDatabaseRow(this.schema, this.databaseRowFactory);
                    updates.Add((unitOfWork.GetRow(operation.GetEntity().GetType(), operation.Collection, operation.GetEntity()), newDatabaseRow));
                    postPersistDocumentUpdates.Add(
                        async () => {
                            unitOfWork.UpdateRow(operation.GetEntity().GetType(), operation.Collection, operation.GetEntity(), newDatabaseRow);
                            this.CallMethod(operation.GetType().GenericTypeArguments, nameof(UpdateMemoryCache), operation.GetEntity(), newDatabaseRow);
                            await (ValueTask)this.CallMethod(
                                operation.GetType().GenericTypeArguments,
                                nameof(UpdateDistributedCacheAsync),
                                operation.GetEntity(),
                                newDatabaseRow,
                                cancellationToken);
                        });
                }
                else {
                    // don't have a new row so just the old one
                    deletes.Add(unitOfWork.GetRow(operation.GetEntity().GetType(), operation.Collection, operation.GetEntity()));
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

            return (ValueTask)this.CallMethod(
                new[] { typeof(TEntity), deleteOperation.Collection.KeyType },
                nameof(this.DeleteFromDistributedCacheAsync),
                deleteOperation,
                deleteOperation.Collection);
        }

        private async ValueTask DeleteFromDistributedCacheAsync<TEntity, TKey>(DeleteOperation<TEntity> deleteOperation, Collection collection, CancellationToken cancellationToken) {
            return;
            //return this.distributedCache.RemoveAsync(CacheKeyProvider.GetCacheKey<TEntity, TKey>(collection, deleteOperation.Entity), cancellationToken);
        }

        private void DeleteFromMemoryCache<TEntity>(DeleteOperation<TEntity> deleteOperation) {
            if (this.memoryCache == null) {
                return;
            }

            this.CallMethod(new[] { typeof(TEntity), deleteOperation.Collection.KeyType }, nameof(DeleteFromMemoryCache), deleteOperation, deleteOperation.Collection);
        }

        private void DeleteFromMemoryCache<TEntity, TKey>(DeleteOperation<TEntity> deleteOperation, Collection collection) {
            return;
            //this.memoryCache.Remove(CacheKeyProvider.GetCacheKey<TEntity, TKey>(collection, deleteOperation.Entity));
        }

        #endregion

        #region UpdateCacheMethods

        private void UpdateMemoryCache<TEntity>(TEntity entity, DatabaseRow row) {
            if (this.memoryCache == null) {
                return;
            }

            this.CallMethod(new[] { typeof(TEntity), row.Collection.KeyType }, nameof(UpdateMemoryCache), entity, row, row.Collection);
        }

        private void UpdateMemoryCache<TEntity, TKey>(TEntity entity, DatabaseRow row, Collection collection) {
            //this.memoryCache.Set(CacheKeyProvider.GetCacheKey<TEntity, TKey>(collection, entity), row.Values);
        }

        private ValueTask UpdateDistributedCacheAsync<TEntity>(TEntity entity, DatabaseRow row, CancellationToken cancellationToken) {
            if (this.distributedCache == null) {
                return ValueTask.CompletedTask;
            }

            return (ValueTask)this.CallMethod(
                new[] { typeof(TEntity), row.Collection.KeyType },
                nameof(UpdateDistributedCacheAsync),
                entity,
                row,
                row.Collection,
                cancellationToken);
        }

        private async ValueTask UpdateDistributedCacheAsync<TEntity, TKey>(TEntity entity, DatabaseRow row, Collection collection, CancellationToken cancellationToken) {
            //return this.distributedCache.SetAsync(CacheKeyProvider.GetCacheKey<TEntity, TKey>(collection, entity), row.Values, cancellationToken);
        }

        #endregion

        public async ValueTask DisposeAsync() {
            if (this.persistenceUpdateExecutor is IAsyncDisposable disposable) {
                await disposable.DisposeAsync();
            }
        }

        public void Dispose() {
            if (this.persistenceUpdateExecutor is IDisposable disposable) {
                disposable.Dispose();
            }
        }
    }
}