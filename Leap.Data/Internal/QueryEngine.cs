namespace Leap.Data.Internal {
    using System;
    using System.Collections;
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
    using Leap.Data.Utilities;

    class QueryEngine : IAsyncDisposable {
        private readonly ISchema schema;

        private readonly IdentityMap identityMap;

        private readonly IQueryExecutor persistenceQueryExecutor;
        
        private readonly ICacheExecutor[] cacheExecutors;

        private IdentityMapExecutor identityMapExecutor;

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
            IQueryExecutor persistenceQueryExecutor,
            ISerializer serializer,
            MemoryCacheExecutor memoryCacheExecutor,
            DistributedCacheExecutor distributedCacheExecutor) {
            this.schema                   = schema;
            this.identityMap              = identityMap;
            this.persistenceQueryExecutor = persistenceQueryExecutor;
            this.serializer               = serializer;
            this.identityMapExecutor      = new IdentityMapExecutor(this.identityMap);
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
            var table = this.schema.GetTable<T>();
            if (!this.identityMapQueries.Contains(query.Identifier) 
                && !this.cacheExecutorQueries.Any(e => e.Contains(query.Identifier)) 
                && !this.persistenceQueryExecutorQueries.Contains(query.Identifier)) {
                // query has not been executed, so let's flush existing queries and then add
                await this.FlushAsync();
                this.Add(query);
            }

            await this.EnsureExecutedAsync();
            if (this.identityMapQueries.Contains(query.Identifier)) {
                await foreach (var document in this.identityMapExecutor.GetAsync<T>(query)) {
                    yield return document.Entity;
                }
                
                yield break;
            }
            
            foreach (var entry in this.cacheExecutors.AsSmartEnumerable()) {
                var cacheExecutor = entry.Value;
                if (this.cacheExecutorQueries[entry.Index].Contains(query.Identifier)) {
                    await foreach (var row in cacheExecutor.GetAsync<T>(query)) {
                        yield return HydrateDocument(row);
                    }
                    
                    yield break;
                }
            }

            await foreach (var row in this.persistenceQueryExecutor.GetAsync<T>(query)) {
                yield return HydrateDocument(row);
            }

            T HydrateDocument(object[] row) {
                // need to hydrate the entity from the database row and add to the document
                var id = table.KeyType.TryCreateInstance(table.Columns.Select(c => c.Name).ToArray(), row);

                // TODO invalidate old versions
                // check ID map for instance
                if (this.identityMap.TryGetValue(table.KeyType, id, out IDocument<T> alreadyMappedDocument)) {
                    return alreadyMappedDocument.Entity;
                }

                var json = RowValueHelper.GetValue<string>(table, row, SpecialColumns.Document);
                var typeName = RowValueHelper.GetValue<string>(table, row, SpecialColumns.DocumentType);
                var documentType = Type.GetType(typeName); // TODO better type handling across assemblies
                if (!(this.serializer.Deserialize(documentType, json) is T entity)) {
                    throw new Exception($"Unable to cast object of type {typeName} to {typeof(T)}");
                }

                this.identityMap.Add(table.KeyType, id, new Document<T>(entity, new DatabaseRow(table, row)) { State = DocumentState.Persisted });
                return entity;
            }
        }

        public async ValueTask EnsureExecutedAsync() {
            if (this.queriesToExecute.Any()) {
                await this.ExecuteAsync();
            }
        }

        private async ValueTask FlushAsync() {
            if (this.persistenceQueryExecutor != null) {
                await this.persistenceQueryExecutor.FlushAsync();
            }
        }

        private async Task ExecuteAsync(CancellationToken cancellationToken = default) {
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