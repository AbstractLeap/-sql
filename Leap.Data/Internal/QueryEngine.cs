namespace Leap.Data.Internal {
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Data;
    using System.Data.Common;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;

    using Fasterflect;

    using Leap.Data.IdentityMap;
    using Leap.Data.Internal.QueryWriter;
    using Leap.Data.Queries;
    using Leap.Data.Schema;

    class QueryEngine : IAsyncDisposable {
        private readonly IConnectionFactory connectionFactory;

        private readonly ISchema schema;

        private readonly IdentityMap identityMap;

        private readonly ISerializer serializer;

        private readonly ISqlQueryWriter sqlQueryWriter;

        /// <summary>
        /// where to store the results of queries not yet enumerated by the user
        /// </summary>
        private readonly ResultCache resultCache;

        /// <summary>
        /// queries to be executed
        /// </summary>
        private readonly Queue<IQuery> queriesToExecute = new Queue<IQuery>();

        /// <summary>
        /// queries that have not been read yet from the db reader
        /// </summary>
        private Queue<IQuery> notReadQueries = new Queue<IQuery>();
        
        private readonly LocalQueryExecutor localQueryExecutor;

        private DbCommand command;

        private DbDataReader dataReader;

        public QueryEngine(
            IConnectionFactory connectionFactory,
            ISchema schema,
            IdentityMap identityMap,
            ISerializer serializer,
            ISqlQueryWriter sqlQueryWriter) {
            this.connectionFactory  = connectionFactory;
            this.schema             = schema;
            this.identityMap        = identityMap;
            this.serializer         = serializer;
            this.localQueryExecutor = new LocalQueryExecutor(this.identityMap);
            this.sqlQueryWriter     = sqlQueryWriter;
            this.resultCache        = new ResultCache();
        }

        public void Add(IQuery query) {
            this.queriesToExecute.Enqueue(query);
        }

        public async IAsyncEnumerable<T> GetResult<T>(IQuery query)
            where T : class {
            // add query if necessary
            if (this.queriesToExecute.All(q => q.Identifier != query.Identifier)) {
                await this.FlushAsync(); // ensure existing command has been fully read
                this.Add(query);
            }

            if (this.queriesToExecute.Any()) {
                await this.ExecuteAsync();
            }

            // do we have the result already?
            if (this.resultCache.TryGetValue<T>(query, out var result)) {
                foreach (var entity in result) {
                    yield return entity;
                }

                yield break;
            }

            // we don't have the result, so we must go through the reader until we do.
            var nonCompleteQuery = this.notReadQueries.Dequeue();
            while (nonCompleteQuery != null && nonCompleteQuery.Identifier != query.Identifier) {
                await this.ReadResultIntoCacheAsync(nonCompleteQuery);
                await this.dataReader.NextResultAsync();
                nonCompleteQuery = this.notReadQueries.Dequeue();
            }

            // read the result we've been asked for
            await foreach (var p in this.ReadResultAsync<T>()) {
                yield return p;
            }

            if (this.notReadQueries.Count == 0) {
                await this.CleanUpCommandAsync();
            }
        }

        private async Task ReadResultIntoCacheAsync(IQuery nonCompleteQuery) {
            var queryResultsTask = (Task)this.CallMethod(new[] { nonCompleteQuery.EntityType }, nameof(this.ReadResultIntoListAsync), Array.Empty<object>());
            await queryResultsTask;
            this.resultCache.Add(nonCompleteQuery, (IList)queryResultsTask.GetPropertyValue("Result"));
        }

        private async Task FlushAsync() {
            while (this.notReadQueries.TryDequeue(out var query)) {
                await this.ReadResultIntoCacheAsync(query);
                await this.dataReader.NextResultAsync();
            }

            if (this.dataReader != null) {
                await this.CleanUpCommandAsync();
            }
        }

        private async Task CleanUpCommandAsync() {
            await this.dataReader.CloseAsync();
            await this.dataReader.DisposeAsync();
            await this.command.DisposeAsync();
        }

        private async Task ExecuteAsync(CancellationToken cancellationToken = default) {
            var nonCompletedQueries = new Queue<IQuery>();
            while (this.queriesToExecute.TryDequeue(out var query)) {
                if (this.localQueryExecutor.CanExecute(query)) {
                    var attemptedResult = await this.localQueryExecutor.ExecuteAsync(query);
                    if (attemptedResult.WasSuccessful) {
                        this.resultCache.Add(query, (IList)attemptedResult.Result);
                    }
                    else {
                        nonCompletedQueries.Enqueue(query);
                    }
                }
                else {
                    nonCompletedQueries.Enqueue(query);
                }
            }

            if (!nonCompletedQueries.Any()) {
                // we've executed all the current queries
                return;
            }

            var connection = this.connectionFactory.Get();
            if (connection.State != ConnectionState.Open) {
                await connection.OpenAsync(cancellationToken).ConfigureAwait(false);
            }

            // TODO figure out connection/transaction lifecycle
            this.command = connection.CreateCommand();
            var queryCommand = new Command();
            foreach (var nonCompletedQuery in nonCompletedQueries) {
                sqlQueryWriter.Write(nonCompletedQuery, queryCommand);
            }

            queryCommand.WriteToDbCommand(this.command);
            this.dataReader     = await this.command.ExecuteReaderAsync(cancellationToken).ConfigureAwait(false);
            this.notReadQueries = nonCompletedQueries;
        }

        private async Task<List<T>> ReadResultIntoListAsync<T>()
            where T : class {
            return await this.ReadResultAsync<T>().ToListAsync();
        }

        private async IAsyncEnumerable<T> ReadResultAsync<T>()
            where T : class {
            while (await this.dataReader.ReadAsync().ConfigureAwait(false)) {
                // NOTE TO SELF you've done the wrong thing. this should be put in the resultBag, not yielded, that should be done later
                // hydrate database row
                var table = this.schema.GetTable<T>();
                var values = new object[table.Columns.Count];
                this.dataReader.GetValues(values);
                var row = new DatabaseRow(table, values);
                var id = table.KeyType.TryCreateInstance(table.Columns.Select(c => c.Name).ToArray(), values);

                // TODO invalidate old versions
                // check ID map for instance
                if (this.identityMap.TryGetValue(table.KeyType, id, out Document<T> document)) {
                    yield return document.Entity;
                    continue;
                }

                var json = row.GetValue<string>(SpecialColumns.Document);
                var typeName = row.GetValue<string>(SpecialColumns.DocumentType);
                var documentType = Type.GetType(typeName); // TODO better type handling across assemblies
                if (!(this.serializer.Deserialize(documentType, json) is T entity)) {
                    throw new Exception($"Unable to cast object of type {typeName} to {typeof(T)}");
                }

                this.identityMap.Add(table.KeyType, id, new Document<T> { Row = row, Entity = entity, State = DocumentState.Persisted });
                yield return entity;

                // TODO second level cache
            }
        }

        public async ValueTask DisposeAsync() {
            if (this.dataReader != null) {
                await this.dataReader.DisposeAsync().ConfigureAwait(false);
            }

            if (this.command != null) {
                await this.command.DisposeAsync().ConfigureAwait(false);
            }
        }
    }
}