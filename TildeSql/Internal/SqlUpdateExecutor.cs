namespace TildeSql.Internal {
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Data.Common;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;

    using TildeSql.Exceptions;
    using TildeSql.Internal.QueryWriter;
    using TildeSql.Internal.UpdateWriter;

    public class SqlUpdateExecutor : IUpdateExecutor, IAsyncDisposable, IDisposable {
        private readonly IConnectionFactory connectionFactory;

        private readonly ISqlUpdateWriter updateWriter;

        private readonly ISqlDialect sqlDialect;

        public SqlUpdateExecutor(IConnectionFactory connectionFactory, ISqlUpdateWriter updateWriter, ISqlDialect sqlDialect) {
            this.connectionFactory = connectionFactory;
            this.updateWriter      = updateWriter;
            this.sqlDialect        = sqlDialect;
        }

        public async ValueTask ExecuteAsync(IEnumerable<DatabaseRow> inserts, IEnumerable<(DatabaseRow OldDatabaseRow, DatabaseRow NewDatabaseRow)> updates, IEnumerable<DatabaseRow> deletes, CancellationToken cancellationToken = default)
        {
            await using var connection = this.connectionFactory.Get();
            if (connection.State != ConnectionState.Open) {
                await connection.OpenAsync(cancellationToken).ConfigureAwait(false);
            }

            await using var transaction = await connection.BeginTransactionAsync(cancellationToken);
            await using var dbCommand = connection.CreateCommand();
            dbCommand.Transaction = transaction;
            var command = new Command();
            var wrapSqlWithAffectedRowsCount = false;
            var returnsData = false;
            command.OnQueryAdded += (sender, args) => {
                if (wrapSqlWithAffectedRowsCount) {
                    args.Query  = this.sqlDialect.AddAffectedRowsCount(args.Query, command);
                    returnsData = true;
                }
            };

            foreach (var databaseRow in inserts) {
                returnsData = returnsData || databaseRow.Collection.IsKeyComputed;
                this.updateWriter.WriteInsert(databaseRow, command);
            }

            wrapSqlWithAffectedRowsCount = true;
            foreach (var update in updates) {
                this.updateWriter.WriteUpdate(update, command);
            }

            foreach (var databaseRow in deletes) {
                this.updateWriter.WriteDelete(databaseRow, command);
            }

            command.WriteToDbCommand(dbCommand);
            if (!returnsData) {
                await dbCommand.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
                await transaction.CommitAsync(cancellationToken);
                return;
            }

            await using (var dbReader = await dbCommand.ExecuteReaderAsync(cancellationToken).ConfigureAwait(false)) {
                List<Exception> exceptions = new();
                foreach (var insert in inserts) {
                    if (!insert.Collection.IsKeyComputed) {
                        continue;
                    }

                    await dbReader.ReadAsync(cancellationToken).ConfigureAwait(false);
                    insert.Values[insert.Collection.GetColumnIndex(insert.Collection.KeyColumns.First().Name)] = dbReader.GetValue(0);
                    await dbReader.NextResultAsync(cancellationToken).ConfigureAwait(false);
                }

                foreach (var update in updates) {
                    await ReadOptimisticConcurrencyResultAsync(dbReader, update.OldDatabaseRow, exceptions);
                }

                foreach (var databaseRow in deletes) {
                    await ReadOptimisticConcurrencyResultAsync(dbReader, databaseRow, exceptions);
                }

                if (exceptions.Any()) {
                    throw new AggregateException(exceptions);
                }
            }

            await transaction.CommitAsync(cancellationToken);
            await connection.CloseAsync();

            async Task ReadOptimisticConcurrencyResultAsync(DbDataReader dbReader, DatabaseRow databaseRow, List<Exception> exceptions) {
                if (!await dbReader.ReadAsync(cancellationToken).ConfigureAwait(false)) {
                    exceptions.Add(new OptimisticConcurrencyException(databaseRow));
                }

                var affectedRows = await dbReader.GetFieldValueAsync<int>(0, cancellationToken);
                if (affectedRows == 0) {
                    exceptions.Add(new OptimisticConcurrencyException(databaseRow));
                }

                await dbReader.NextResultAsync(cancellationToken).ConfigureAwait(false);
            }
        }

        public async ValueTask DisposeAsync() {
            if (this.connectionFactory is IAsyncDisposable disposableConnectionFactory) {
                await disposableConnectionFactory.DisposeAsync();
            }
        }

        public void Dispose() {
            if (this.connectionFactory is IDisposable disposableConnectionFactory) {
                disposableConnectionFactory.Dispose();
            }
        }
    }
}