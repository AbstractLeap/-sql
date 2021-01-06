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

    class QueryEngine {
        private readonly ISchema schema;

        private readonly IdentityMap identityMap;

        private readonly IQueryExecutor persistenceQueryExecutor;

        private readonly ISerializer serializer;

        /// <summary>
        ///     queries to be executed
        /// </summary>
        private readonly List<IQuery> queriesToExecute = new List<IQuery>();

        private readonly HashSet<Guid> localQueryExecutorQueries = new HashSet<Guid>();

        private readonly HashSet<Guid> persistenceQueryExecutorQueries = new HashSet<Guid>();

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
            if (!this.localQueryExecutorQueries.Contains(query.Identifier) && !this.persistenceQueryExecutorQueries.Contains(query.Identifier)) {
                // query has not been executed, so let's flush existing queries and then add
                await this.FlushAsync();
                this.Add(query);
            }

            await this.EnsureExecutedAsync();
            if (this.localQueryExecutorQueries.Contains(query.Identifier)) {
                await foreach (var document in this.localQueryExecutor.GetAsync<T>(query)) {
                    yield return document.Entity;
                }
                
                yield break;
            }

            var table = this.schema.GetTable<T>();
            await foreach (var row in this.persistenceQueryExecutor.GetAsync<T>(query)) {
                // need to hydrate the entity from the database row and add to the document
                var id = table.KeyType.TryCreateInstance(table.Columns.Select(c => c.Name).ToArray(), row);

                // TODO invalidate old versions
                // check ID map for instance
                if (this.identityMap.TryGetValue(table.KeyType, id, out Document<T> alreadyMappedDocument)) {
                    yield return alreadyMappedDocument.Entity;
                    continue;
                }

                var json = RowValueHelper.GetValue<string>(table, row, SpecialColumns.Document);
                var typeName = RowValueHelper.GetValue<string>(table, row, SpecialColumns.DocumentType);
                var documentType = Type.GetType(typeName); // TODO better type handling across assemblies
                if (!(this.serializer.Deserialize(documentType, json) is T entity)) {
                    throw new Exception($"Unable to cast object of type {typeName} to {typeof(T)}");
                }

                this.identityMap.Add(table.KeyType, id, new Document<T>(new DatabaseRow(table, row), entity) { State = DocumentState.Persisted });
                yield return entity;

                // TODO second level cache
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
            var localExecutionResult = await this.localQueryExecutor.ExecuteAsync(queriesStillToExecute, cancellationToken);
            foreach (var executedQuery in localExecutionResult.ExecutedQueries) {
                this.localQueryExecutorQueries.Add(executedQuery.Identifier);
            }

            queriesStillToExecute = localExecutionResult.NonExecutedQueries;

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
    }
}