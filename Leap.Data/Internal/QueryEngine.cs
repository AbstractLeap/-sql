﻿namespace Leap.Data.Internal {
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;

    using Fasterflect;

    using Leap.Data.IdentityMap;
    using Leap.Data.Internal.Caching;
    using Leap.Data.Queries;
    using Leap.Data.Schema;
    using Leap.Data.Serialization;
    using Leap.Data.UnitOfWork;
    using Leap.Data.Utilities;

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

        private readonly HashSet<Guid> persistenceQueryExecutorQueries = new();

        private readonly HashSet<Guid>[] cacheExecutorQueries;

        private readonly HashSet<Guid> identityMapQueries = new();

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
            this.cacheExecutorQueries     = this.cacheExecutors.Select(_ => new HashSet<Guid>()).ToArray();

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
            if (!this.identityMapQueries.Contains(query.Identifier) 
                && !this.cacheExecutorQueries.Any(e => e.Contains(query.Identifier)) 
                && !this.persistenceQueryExecutorQueries.Contains(query.Identifier)) {
                // query has not been executed, so let's flush existing queries and then add
                await this.FlushAsync();
                this.Add(query);
            }

            await this.ExecuteAsync();
            if (this.identityMapQueries.Contains(query.Identifier)) {
                await foreach (var entity in this.identityMapExecutor.GetAsync<T>(query)) {
                    yield return entity;
                }
                
                yield break;
            }
            
            // TODO fix caches getting instances from the wrong collection
            foreach (var entry in this.cacheExecutors.AsSmartEnumerable()) {
                var cacheExecutor = entry.Value;
                if (this.cacheExecutorQueries[entry.Index].Contains(query.Identifier)) {
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
                var id = CreateIdInstance<T>(collection, row);

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
                var documentType = Type.GetType(typeName); // TODO better type handling across assemblies
                if (this.serializer.Deserialize(documentType, json) is not T entity) {
                    throw new Exception($"Unable to cast object of type {typeName} to {typeof(T)}");
                }

                this.identityMap.Add(collection.KeyType, id, entity);
                this.unitOfWork.AddOrUpdate(collection, entity, new DatabaseRow(collection, row), DocumentState.Persisted);
                return entity;
            }
        }

        private static object CreateIdInstance<T>(Collection collection, object[] row)
            where T : class {
            object id;
            if (collection.KeyType.IsPrimitiveType()) {
                id = row[0];
            }
            else if (collection.KeyMembers.Length == 1) {
                // TODO make this not use all of the columns
                id = collection.KeyType.TryCreateInstance(collection.Columns.Select(c => c.Name).ToArray(), row);
            }
            else {
                // tuple type keys
                var compositeKeys = new object[collection.KeyMembers.Length];
                foreach (var entry in collection.KeyMembers.AsSmartEnumerable()) {
                    var keyMember = entry.Value;
                    if (keyMember.PropertyOrFieldType().IsPrimitiveType()) {
                        compositeKeys[entry.Index] = RowValueHelper.GetValue(keyMember.PropertyOrFieldType(), collection, row, keyMember.Name);
                    }
                    else {
                        var keyMemberKeyColumns = collection.KeyColumns.Where(c => c.KeyMemberInfo.Equals(keyMember)).ToArray();
                        var columnValues = keyMemberKeyColumns.Select(c => RowValueHelper.GetValue(c.Type, collection, row, c.Name)).ToArray();
                        compositeKeys[entry.Index] = keyMember.PropertyOrFieldType().TryCreateInstance(keyMemberKeyColumns.Select(c => c.Name).ToArray(), columnValues);
                    }
                }

                id = collection.KeyType.TryCreateInstance(collection.KeyMembers.Select((m, i) => $"item{i + 1}").ToArray(), compositeKeys);
            }

            return id;
        }

        public async ValueTask EnsureCleanAsync(CancellationToken cancellationToken = default) {
            await this.FlushAsync(); // clear out any non-read queries
            await this.ExecuteAsync(cancellationToken); // execute any non-executed queries
            await this.FlushAsync(); // ensure that they're also read
        }

        /// <summary>
        ///     Ensures that no further processing needs to be done by the persistence
        ///     (makes the persistence ready to accept new requests)
        /// </summary>
        /// <returns></returns>
        private async ValueTask FlushAsync() {
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
                this.identityMapQueries.Add(executedQuery.Identifier);
            }

            queriesStillToExecute = identityMapExecutionResult.NonExecutedQueries;

            if (queriesStillToExecute.Any()) {
                foreach (var entry in this.cacheExecutors.AsSmartEnumerable()) {
                    var cacheExecutionResult = await entry.Value.ExecuteAsync(queriesStillToExecute, cancellationToken);
                    foreach (var executedQuery in cacheExecutionResult.ExecutedQueries) {
                        this.cacheExecutorQueries[entry.Index].Add(executedQuery.Identifier);
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
                    this.persistenceQueryExecutorQueries.Add(executedQuery.Identifier);
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