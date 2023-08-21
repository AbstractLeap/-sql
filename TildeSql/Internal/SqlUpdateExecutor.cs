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
            await using var connection = await this.connectionFactory.GetAsync();
            if (connection.State != ConnectionState.Open) {
                await connection.OpenAsync(cancellationToken).ConfigureAwait(false);
            }

            await using var transaction = await connection.BeginTransactionAsync(cancellationToken);

            // in SQL server we have a 2100 parameter limit so we execute multiple commands as necessary (inside the same transaction)
            var insertList = inserts as List<DatabaseRow> ?? inserts.ToList(); // this is true by default
            var updateList = updates as List<(DatabaseRow OldDatabaseRow, DatabaseRow NewDatabaseRow)> ?? updates.ToList();
            var deleteList = deletes as List<DatabaseRow> ?? deletes.ToList();
            var insertIdx = 0;
            var updateIdx = 0;
            var deleteIdx = 0;
            while (insertIdx < insertList.Count || updateIdx < updateList.Count || deleteIdx < deleteList.Count) {
                var startInsertIdx = insertIdx;
                var startUpdateIdx = updateIdx;
                var startDeleteIdx = deleteIdx;
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

                for (; insertIdx < insertList.Count && command.ParameterCount < 2000; insertIdx++) {
                    var databaseRow = insertList[insertIdx];
                    returnsData = returnsData || databaseRow.Collection.IsKeyComputed;
                    this.updateWriter.WriteInsert(databaseRow, command);
                }

                wrapSqlWithAffectedRowsCount = true;
                for (; updateIdx < updateList.Count && command.ParameterCount < 2000; updateIdx++) {
                    this.updateWriter.WriteUpdate(updateList[updateIdx], command);
                }

                for (; deleteIdx < deleteList.Count && command.ParameterCount < 2000; deleteIdx++) {
                    this.updateWriter.WriteDelete(deleteList[deleteIdx], command);
                }

                command.WriteToDbCommand(dbCommand);
                if (!returnsData) {
                    // we can just execute and then run the next iteration if necessary
                    await dbCommand.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
                }
                else {
                    await using var dbReader = await dbCommand.ExecuteReaderAsync(cancellationToken).ConfigureAwait(false);
                    List<Exception> exceptions = new();
                    for (var i = startInsertIdx; i < insertIdx; i++) {
                        var insert = insertList[i];
                        if (!insert.Collection.IsKeyComputed) {
                            continue;
                        }

                        await dbReader.ReadAsync(cancellationToken).ConfigureAwait(false);
                        insert.Values[insert.Collection.GetColumnIndex(insert.Collection.KeyColumns.First().Name)] = dbReader.GetValue(0);
                        await dbReader.NextResultAsync(cancellationToken).ConfigureAwait(false);
                    }

                    for (var i = startUpdateIdx; i < updateIdx; i++) {
                        await ReadOptimisticConcurrencyResultAsync(dbReader, updateList[i].OldDatabaseRow, exceptions);
                    }

                    for (var i = startDeleteIdx; i < deleteIdx; i++) {
                        await ReadOptimisticConcurrencyResultAsync(dbReader, deleteList[i], exceptions);
                    }

                    if (exceptions.Any()) {
                        throw new AggregateException(exceptions);
                    }
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