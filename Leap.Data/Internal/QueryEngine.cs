namespace Leap.Data.Internal {
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;

    using Fasterflect;

    using Leap.Data.IdentityMap;
    using Leap.Data.Queries;
    using Leap.Data.Schema;
    using Leap.Data.Serialization;

    class QueryEngine : IAsyncDisposable {
        private readonly ISchema schema;

        private readonly IdentityMap identityMap;

        private readonly IQueryExecutor persistenceQueryExecutor;

        private readonly ISerializer serializer;

        /// <summary>
        ///     queries to be executed
        /// </summary>
        private readonly List<IQuery> queriesToExecute = new List<IQuery>();

        private readonly IDictionary<Guid, IQueryGetter> queryExecutorLookup = new Dictionary<Guid, IQueryGetter>();

        private readonly LocalQueryExecutor localQueryExecutor;

        public QueryEngine(ISchema schema, IdentityMap identityMap, IQueryExecutor persistenceQueryExecutor, ISerializer serializer) {
            this.schema                   = schema;
            this.identityMap              = identityMap;
            this.persistenceQueryExecutor = persistenceQueryExecutor;
            this.serializer               = serializer;
            this.localQueryExecutor       = new LocalQueryExecutor(this.identityMap);
        }

        public void Add(IQuery query) {
            this.queriesToExecute.Add(query);
        }

        public async IAsyncEnumerable<T> GetResult<T>(IQuery query)
            where T : class {
            if (!this.queryExecutorLookup.TryGetValue(query.Identifier, out var executor)) {
                // query has not been executed, so let's flush existing queries and then add
                await this.FlushAsync();
                this.Add(query);
            }

            if (this.queriesToExecute.Any()) {
                await this.ExecuteAsync();
            }

            if (!this.queryExecutorLookup.TryGetValue(query.Identifier, out executor)) {
                throw new Exception("Query has not been executed");
            }

            var table = this.schema.GetTable<T>();
            await foreach (var document in executor.GetAsync<T>(query)) {
                if (document.Entity != null) {
                    yield return document.Entity;
                    continue;
                }

                // need to hydrate the entity from the database row and add to the document
                var id = table.KeyType.TryCreateInstance(table.Columns.Select(c => c.Name).ToArray(), document.Row.Values);

                // TODO invalidate old versions
                // check ID map for instance
                if (this.identityMap.TryGetValue(table.KeyType, id, out Document<T> alreadyMappedDocument)) {
                    yield return alreadyMappedDocument.Entity;
                    continue;
                }

                var json = document.Row.GetValue<string>(SpecialColumns.Document);
                var typeName = document.Row.GetValue<string>(SpecialColumns.DocumentType);
                var documentType = Type.GetType(typeName); // TODO better type handling across assemblies
                if (!(this.serializer.Deserialize(documentType, json) is T entity)) {
                    throw new Exception($"Unable to cast object of type {typeName} to {typeof(T)}");
                }

                this.identityMap.Add(table.KeyType, id, new Document<T>(document.Row, entity) { State = DocumentState.Persisted });
                yield return entity;

                // TODO second level cache
            }
        }

        private async ValueTask FlushAsync() {
            if (this.persistenceQueryExecutor != null) {
                await this.persistenceQueryExecutor.FlushAsync();
            }
        }

        private async Task ExecuteAsync(CancellationToken cancellationToken = default) {
            IEnumerable<IQuery> queriesStillToExecute = this.queriesToExecute;
            var localExecutionResult = await this.localQueryExecutor.ExecuteAsync(queriesStillToExecute, cancellationToken);
            foreach (var executedQuery in localExecutionResult.ExecutedQueries) {
                this.queryExecutorLookup.Add(executedQuery.Identifier, this.localQueryExecutor);
            }

            queriesStillToExecute = localExecutionResult.NonExecutedQueries;

            if (queriesStillToExecute.Any()) {
                if (this.persistenceQueryExecutor == null) {
                    throw new Exception("No persistence query mechanism has been configured");
                }

                await this.persistenceQueryExecutor.ExecuteAsync(queriesStillToExecute, cancellationToken);
                foreach (var executedQuery in queriesStillToExecute) {
                    this.queryExecutorLookup.Add(executedQuery.Identifier, this.persistenceQueryExecutor);
                }
            }
            
            this.queriesToExecute.Clear();
        }

        public async ValueTask DisposeAsync() {
            if (this.persistenceQueryExecutor != null && this.persistenceQueryExecutor is IAsyncDisposable disposablePersistenceQueryExecutor) {
                await disposablePersistenceQueryExecutor.DisposeAsync();
            }
        }
    }
}