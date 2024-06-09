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

        private readonly CacheSetter cacheSetter;

        public UpdateEngine(IUpdateExecutor persistenceUpdateExecutor, IMemoryCache memoryCache, IDistributedCache distributedCache, ISchema schema, ISerializer serializer,
                            ICacheSerializer cacheSerializer,
                            CacheOptions cacheOptions) {
            this.persistenceUpdateExecutor = persistenceUpdateExecutor;
            this.memoryCache               = memoryCache;
            this.distributedCache          = distributedCache;
            this.schema                    = schema;
            this.serializer                = serializer;
            this.databaseRowFactory        = new DatabaseRowFactory(serializer);
            if (this.memoryCache != null || this.distributedCache != null) {
                this.cacheSetter = new CacheSetter(this.memoryCache, this.distributedCache, cacheSerializer, cacheOptions);
            }
        }

        public async ValueTask ExecuteAsync(UnitOfWork unitOfWork, CancellationToken cancellationToken = default) {
            var operations = unitOfWork.Operations.ToArray();
            if (!operations.Any()) {
                return;
            }

            // we delete from cache now, as failure on the persistence is ok
            foreach (var operation in operations) {
                if (operation.IsDeleteOperation()) {
                    if (this.cacheSetter != null) {
                        await (ValueTask)cacheSetter.CallMethod(operation.GetType().GenericTypeArguments, nameof(CacheSetter.RemoveAsync), operation.GetEntity(), operation.Collection);
                    }
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
                            if (this.cacheSetter != null) {
                                await (ValueTask)cacheSetter.CallMethod([operation.GetType().GenericTypeArguments.Single(), operation.Collection.KeyType], nameof(CacheSetter.SetAsync), operation.GetEntity(), operation.Collection, newDatabaseRow);
                            }
                        });
                }
                else if (operation.IsUpdateOperation()) {
                    // need to provide existing row and new row (e.g. to support optimistic concurrency
                    var newDatabaseRow = operation.GetNewDatabaseRow(this.schema, this.databaseRowFactory);
                    updates.Add((unitOfWork.GetRow(operation.GetEntity().GetType(), operation.Collection, operation.GetEntity()), newDatabaseRow));
                    postPersistDocumentUpdates.Add(
                        async () => {
                            unitOfWork.UpdateRow(operation.GetEntity().GetType(), operation.Collection, operation.GetEntity(), newDatabaseRow);
                            if (this.cacheSetter != null) {
                                await (ValueTask)cacheSetter.CallMethod([operation.GetType().GenericTypeArguments.Single(), operation.Collection.KeyType], nameof(CacheSetter.SetAsync), operation.GetEntity(), operation.Collection, newDatabaseRow);
                            }
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