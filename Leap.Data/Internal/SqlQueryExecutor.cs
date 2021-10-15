namespace Leap.Data.Internal {
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Data.Common;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;

    using Fasterflect;

    using Leap.Data.Internal.QueryWriter;
    using Leap.Data.Queries;
    using Leap.Data.Schema;

    public class SqlQueryExecutor : IQueryExecutor, IAsyncDisposable {
        private readonly IConnectionFactory connectionFactory;

        private readonly ISqlQueryWriter sqlQueryWriter;

        private readonly ISchema schema;

        /// <summary>
        ///     where to store the results of queries not yet enumerated by the user
        /// </summary>
        private readonly DatabaseRowResultCache resultCache;

        private DbConnection connection;

        private DbCommand command;

        private DbDataReader dataReader;

        public SqlQueryExecutor(IConnectionFactory connectionFactory, ISqlQueryWriter sqlQueryWriter, ISchema schema) {
            this.connectionFactory = connectionFactory;
            this.sqlQueryWriter    = sqlQueryWriter;
            this.schema            = schema;
            this.resultCache       = new DatabaseRowResultCache();
        }

        /// <summary>
        ///     queries that have not been read yet from the db reader
        /// </summary>
        private readonly Queue<IQuery> notReadQueries = new Queue<IQuery>();

        public async ValueTask ExecuteAsync(IEnumerable<IQuery> queries, CancellationToken cancellationToken = default) {
            if (!queries.Any()) {
                return;
            }

            this.connection = this.connectionFactory.Get();
            if (connection.State != ConnectionState.Open) {
                await connection.OpenAsync(cancellationToken).ConfigureAwait(false);
            }

            // TODO figure out connection/transaction lifecycle
            this.command = connection.CreateCommand();
            var queryCommand = new Command();
            foreach (var nonCompletedQuery in queries) {
                this.sqlQueryWriter.Write(nonCompletedQuery, queryCommand);
                this.notReadQueries.Enqueue(nonCompletedQuery);
            }

            queryCommand.WriteToDbCommand(this.command);
            this.dataReader = await this.command.ExecuteReaderAsync(cancellationToken).ConfigureAwait(false);
        }

        public async IAsyncEnumerable<object[]> GetAsync<TEntity>(IQuery query)
            where TEntity : class {
            // do we have the result already?
            // (we've already read this result from the resultsets)
            if (this.resultCache.TryGetValue(query, out var result)) {
                foreach (var row in result) {
                    yield return row;
                }

                yield break;
            }

            // we don't have the result, so we must go through the reader until we do.
            var nonCompleteQuery = this.notReadQueries.Dequeue();
            while (nonCompleteQuery != null && !nonCompleteQuery.Equals(query)) {
                await this.ReadResultIntoCacheAsync(nonCompleteQuery);
                await this.dataReader.NextResultAsync();
                nonCompleteQuery = this.notReadQueries.Dequeue();
            }

            // read the result we've been asked for
            await foreach (var row in this.ReadResultAsync<TEntity>()) {
                yield return row;
            }

            if (this.notReadQueries.Count == 0) {
                await this.CleanUpCommandAsync();
            }
        }

        private async ValueTask ReadResultIntoCacheAsync(IQuery nonCompleteQuery) {
            var queryResults = await (ValueTask<List<object[]>>)this.CallMethod(new[] { nonCompleteQuery.EntityType }, nameof(this.ReadResultIntoListAsync), Array.Empty<object>());
            this.resultCache.Add(nonCompleteQuery, queryResults);
        }

        /// <summary>
        ///     Reads all result sets from the command in to memory
        /// </summary>
        /// <returns></returns>
        public async ValueTask FlushAsync() {
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
            await this.connection.CloseAsync();
            await this.connection.DisposeAsync();
        }

        private async ValueTask<List<object[]>> ReadResultIntoListAsync<T>()
            where T : class {
            return await this.ReadResultAsync<T>().ToListAsync();
        }

        private async IAsyncEnumerable<object[]> ReadResultAsync<T>()
            where T : class {
            while (await this.dataReader.ReadAsync().ConfigureAwait(false)) {
                // hydrate database row
                var values = new object[this.dataReader.VisibleFieldCount];
                this.dataReader.GetValues(values);
                yield return values;
            }
        }

        public async ValueTask DisposeAsync() {
            if (this.dataReader != null) {
                await this.dataReader.DisposeAsync().ConfigureAwait(false);
            }

            if (this.command != null) {
                await this.command.DisposeAsync().ConfigureAwait(false);
            }

            if (this.connectionFactory is IAsyncDisposable disposableConnectionFactory) {
                await disposableConnectionFactory.DisposeAsync();
            }
        }
    }
}