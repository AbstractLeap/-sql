namespace TildeSql.Internal {
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;

    using TildeSql.Utilities;

    using TildeSql.IdentityMap;
    using TildeSql.Internal.Caching;
    using TildeSql.Queries;
    using TildeSql.Schema;
    using TildeSql.Serialization;
    using TildeSql.UnitOfWork;

    class QueryEngine : IAsyncDisposable {
        private readonly ISchema schema;

        private readonly IdentityMap identityMap;

        private readonly UnitOfWork unitOfWork;

        private readonly IQueryExecutor persistenceQueryExecutor;
        
        private readonly ICacheExecutor[] cacheExecutors;

        private readonly IdentityMapExecutor identityMapExecutor;

        private readonly ISerializer serializer;

        /// <summary>
        ///     queries to be executed
        /// </summary>
        private readonly List<IQuery> queriesToExecute = new();

        private readonly HashSet<IQuery> persistenceQueryExecutorQueries = new();

        private readonly HashSet<IQuery>[] cacheExecutorQueries;

        private readonly HashSet<IQuery> identityMapQueries = new();

        public QueryEngine(
            ISchema schema,
            IdentityMap identityMap,
            UnitOfWork unitOfWork,
            IQueryExecutor persistenceQueryExecutor,
            ISerializer serializer,
            MemoryCacheExecutor memoryCacheExecutor,
            DistributedCacheExecutor distributedCacheExecutor) {
            this.schema                   = schema;
            this.identityMap              = identityMap;
            this.unitOfWork               = unitOfWork;
            this.persistenceQueryExecutor = persistenceQueryExecutor;
            this.serializer               = serializer;
            this.identityMapExecutor      = new IdentityMapExecutor(this.identityMap, unitOfWork);
            this.cacheExecutors           = GetNonNullCacheExecutors().ToArray();
            this.cacheExecutorQueries     = this.cacheExecutors.Select(_ => new HashSet<IQuery>()).ToArray();

            IEnumerable<ICacheExecutor> GetNonNullCacheExecutors() {
                if (memoryCacheExecutor != null) {
                    yield return memoryCacheExecutor;
                }

                if (distributedCacheExecutor != null) {
                    yield return distributedCacheExecutor;
                }
            }
        }

        public void Add(IQuery query) {
            this.queriesToExecute.Add(query);
        }

        public async IAsyncEnumerable<T> GetResult<T>(IQuery query)
            where T : class {
            if (!this.identityMapQueries.Contains(query) && !this.cacheExecutorQueries.Any(e => e.Contains(query)) && !this.persistenceQueryExecutorQueries.Contains(query)) {
                // query has not been executed, so let's flush existing queries and then add
                await this.FlushPersistenceAsync();
                if (!this.queriesToExecute.Contains(query)) {
                    this.Add(query);
                }

                await this.ExecuteAsync();
            }

            if (this.identityMapQueries.Contains(query)) {
                await foreach (var entity in this.identityMapExecutor.GetAsync<T>(query)) {
                    yield return entity;
                }

                yield break;
            }

            // TODO fix caches getting instances from the wrong collection
            foreach (var entry in this.cacheExecutors.AsSmartEnumerable()) {
                var cacheExecutor = entry.Value;
                if (this.cacheExecutorQueries[entry.Index].Contains(query)) {
                    await foreach (var row in cacheExecutor.GetAsync<T>(query)) {
                        var entity = HydrateDocument(row);
                        if (entity != null) {
                            yield return entity;
                        }
                    }

                    yield break;
                }
            }

            await foreach (var row in this.persistenceQueryExecutor.GetAsync<T>(query)) {
                var entity = HydrateDocument(row);
                if (entity != null) {
                    yield return entity;
                }
            }

            T HydrateDocument(object[] row) {
                // need to hydrate the entity from the database row and add to the document
                var collection = query.Collection;
                var id = collection.KeyFactory.Create(row);

                // TODO invalidate old versions
                // check ID map for instance
                if (this.identityMap.TryGetValue(collection.KeyType, id, out T entityInstance)) {
                    if (this.unitOfWork.GetState(collection, entityInstance) == DocumentState.Deleted) {
                        return null;
                    }
                    
                    this.unitOfWork.AddOrUpdate(collection, entityInstance, new DatabaseRow(collection, row), DocumentState.Persisted);
                    return entityInstance;
                }

                var json = RowValueHelper.GetValue<string>(collection, row, SpecialColumns.Document);
                var typeName = RowValueHelper.GetValue<string>(collection, row, SpecialColumns.DocumentType);
                var documentType = collection.GetTypeFromName(typeName);
                documentType = documentType.IsGenericTypeDefinition ? documentType.MakeGenericType(typeof(T).GenericTypeArguments) : documentType;
                if (this.serializer.Deserialize(documentType, json) is not T entity) {
                    throw new Exception($"Unable to cast object of type {typeName} to {typeof(T)}");
                }

                this.identityMap.Add(collection.KeyType, id, entity);
                this.unitOfWork.AddOrUpdate(collection, entity, new DatabaseRow(collection, row), DocumentState.Persisted);
                return entity;
            }
        }

        public async ValueTask EnsureCleanAsync(CancellationToken cancellationToken = default) {
            await this.FlushPersistenceAsync(); // clear out any non-read queries
            await this.ExecuteAsync(cancellationToken); // execute any non-executed queries
            await this.FlushPersistenceAsync(); // ensure that they're also read
        }

        /// <summary>
        ///     Ensures that no further processing needs to be done by the persistence
        ///     (makes the persistence ready to accept new requests)
        /// </summary>
        /// <returns></returns>
        private async ValueTask FlushPersistenceAsync() {
            if (this.persistenceQueryExecutor != null) {
                await this.persistenceQueryExecutor.FlushAsync();
            }
        }

        /// <summary>
        ///     Executes the queries against the various caches and the persistent layer
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        private async Task ExecuteAsync(CancellationToken cancellationToken = default) {
            if (!this.queriesToExecute.Any()) {
                return;
            }

            IEnumerable<IQuery> queriesStillToExecute = this.queriesToExecute;
            var identityMapExecutionResult = this.identityMapExecutor.Execute(queriesStillToExecute, cancellationToken);
            foreach (var executedQuery in identityMapExecutionResult.ExecutedQueries) {
                this.identityMapQueries.Add(executedQuery);
            }

            queriesStillToExecute = identityMapExecutionResult.NonExecutedQueries;

            if (queriesStillToExecute.Any()) {
                foreach (var entry in this.cacheExecutors.AsSmartEnumerable()) {
                    var cacheExecutionResult = await entry.Value.ExecuteAsync(queriesStillToExecute, cancellationToken);
                    foreach (var executedQuery in cacheExecutionResult.ExecutedQueries) {
                        this.cacheExecutorQueries[entry.Index].Add(executedQuery);
                    }

                    queriesStillToExecute = cacheExecutionResult.NonExecutedQueries;
                    if (!queriesStillToExecute.Any()) {
                        break;
                    }
                }
            }

            if (queriesStillToExecute.Any()) {
                if (this.persistenceQueryExecutor == null) {
                    throw new Exception("No persistence query mechanism has been configured");
                }

                await this.persistenceQueryExecutor.ExecuteAsync(queriesStillToExecute, cancellationToken);
                foreach (var executedQuery in queriesStillToExecute) {
                    this.persistenceQueryExecutorQueries.Add(executedQuery);
                }
            }

            this.queriesToExecute.Clear();
        }

        public async ValueTask DisposeAsync() {
            if (this.persistenceQueryExecutor is IAsyncDisposable disposable) {
                await disposable.DisposeAsync();
            }
        }
    }
}