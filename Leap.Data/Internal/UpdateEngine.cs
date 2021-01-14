namespace Leap.Data.Internal {
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;

    using Fasterflect;

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

        private ColumnValueFactoryFactory columnValueFactoryFactory;

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

            var inserts = new List<DatabaseRow>();
            var updates = new List<DatabaseRow>();
            var deletes = new List<DatabaseRow>();
            foreach (var operation in unitOfWork.Operations) {
                List<DatabaseRow> list;
                if (operation.IsAddOperation()) {
                    // only have an entity, need to generate database row
                    list = inserts;
                } else if (operation.IsUpdateOperation()) {
                    list = updates;
                }
                else {
                    list = deletes;
                }

                list.Add((DatabaseRow)this.CallMethod(operation.GetType().GenericTypeArguments, nameof(GetRowFromOperation), operation));
            }
            
            await this.persistenceUpdateExecutor.ExecuteAsync(inserts, updates, deletes, cancellationToken);
            
            // now we update the caches for additions and updates now
            // if the above fails we won't get here which is good
            foreach (var operation in unitOfWork.Operations) {
                if (operation.IsAddOperation() || operation.IsUpdateOperation()) {
                    this.CallMethod(operation.GetType().GenericTypeArguments, nameof(UpdateMemoryCache), operation);
                    await (ValueTask)this.CallMethod(operation.GetType().GenericTypeArguments, nameof(UpdateDistributedCacheAsync), operation, cancellationToken);
                }
            }
        }

        private DatabaseRow GetRowFromOperation<TEntity>(IOperation<TEntity> operation) {
            var table = this.schema.GetTable<TEntity>();
            return (DatabaseRow)this.CallMethod(new[] { typeof(TEntity), table.KeyType }, nameof(GetRowFromOperation), operation, table);
        }

        private DatabaseRow GetRowFromOperation<TEntity, TKey>(IOperation<TEntity> operation, Table table) {
            var key = table.KeyExtractor.Extract<TEntity, TKey>(operation.Document.Entity);
            var values = new object[table.Columns.Count];
            foreach (var keyColumn in table.KeyColumns) {
                values[table.GetColumnIndex(keyColumn.Name)] = table.KeyColumnValueExtractor.GetValue<TEntity, TKey>(keyColumn, key);
            }

            // TODO figure out optimistic concurrency before and after. Need to pass before in to updater so as to do optimistic concurrency but need after back out to store in caches
            foreach (var nonKeyColumn in table.NonKeyColumns) {
                var columnIndex = table.GetColumnIndex(nonKeyColumn.Name);
                if (nonKeyColumn == table.OptimisticConcurrencyColumn
                    && operation.Document.Row != null) {
                    // optimistic concurrency column must be passed existing value
                    values[columnIndex] = operation.Document.Row.Values[columnIndex];
                }
                else {
                    values[table.GetColumnIndex(nonKeyColumn.Name)] = this.columnValueFactoryFactory.GetFactory(nonKeyColumn)
                                                                          .GetValue<TEntity, TKey>(nonKeyColumn, operation.Document.Entity, operation.Document);
                }
            }

            return new DatabaseRow(table, values);
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
            this.memoryCache.Set(key, operation.Document.Row);
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
            return this.distributedCache.SetAsync(key, operation.Document.Row, cancellationToken);
        }

        #endregion
    }
}