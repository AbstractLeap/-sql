namespace Leap.Data.Internal {
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Data.Common;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;

    using Leap.Data.Exceptions;
    using Leap.Data.Internal.QueryWriter;
    using Leap.Data.Internal.UpdateWriter;
    using Leap.Data.Operations;

    public class SqlUpdateExecutor : IUpdateExecutor {
        private readonly IConnectionFactory connectionFactory;

        private readonly ISqlUpdateWriter updateWriter;

        private readonly ISqlDialect sqlDialect;

        public SqlUpdateExecutor(IConnectionFactory connectionFactory, ISqlUpdateWriter updateWriter, ISqlDialect sqlDialect) {
            this.connectionFactory = connectionFactory;
            this.updateWriter      = updateWriter;
            this.sqlDialect        = sqlDialect;
        }

        public async ValueTask ExecuteAsync(IEnumerable<DatabaseRow> inserts, IEnumerable<DatabaseRow> updates, IEnumerable<DatabaseRow> deletes, CancellationToken cancellationToken = default)
        {
            var connection = this.connectionFactory.Get();
            if (connection.State != ConnectionState.Open) {
                await connection.OpenAsync(cancellationToken).ConfigureAwait(false);
            }

            await using (var transaction = await connection.BeginTransactionAsync(cancellationToken))
            await using (var dbCommand = connection.CreateCommand()) {
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
                    this.updateWriter.WriteInsert(databaseRow, command);
                }

                wrapSqlWithAffectedRowsCount = true;
                foreach (var databaseRow in updates) {
                    this.updateWriter.WriteUpdate(databaseRow, command);
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
                    foreach (var databaseRow in updates) {
                        await ReadResultAsync(dbReader, databaseRow, exceptions);
                    }

                    foreach (var databaseRow in deletes) {
                        await ReadResultAsync(dbReader, databaseRow, exceptions);
                    }

                    if (exceptions.Any()) {
                        throw new AggregateException(exceptions);
                    }
                }

                await transaction.CommitAsync(cancellationToken);
            }

            async Task ReadResultAsync(DbDataReader dbReader, DatabaseRow databaseRow, List<Exception> exceptions) {
                if (!await dbReader.ReadAsync(cancellationToken).ConfigureAwait(false)) {
                    AddException(ref exceptions, databaseRow);
                }

                var affectedRows = await dbReader.GetFieldValueAsync<int>(0, cancellationToken);
                if (affectedRows == 0) {
                    AddException(ref exceptions, databaseRow);
                }

                await dbReader.NextResultAsync(cancellationToken).ConfigureAwait(false);
            }

            void AddException(ref List<Exception> exceptions, DatabaseRow databaseRow) {
                exceptions.Add(new OptimisticConcurrencyException(databaseRow));
            }
        }
    }
}