namespace Leap.Data.Internal {
    using System;
    using System.Collections.Generic;
    using System.Data;
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

        public async ValueTask ExecuteAsync(IEnumerable<IOperation> operations, CancellationToken cancellationToken = default) {
            var connection = this.connectionFactory.Get();
            if (connection.State != ConnectionState.Open) {
                await connection.OpenAsync(cancellationToken).ConfigureAwait(false);
            }

            await using (var transaction = await connection.BeginTransactionAsync(cancellationToken))
            await using (var dbCommand = connection.CreateCommand()) {
                dbCommand.Transaction = transaction;
                var command = new Command();
                var wrapSql = false;
                command.OnQueryAdded += (sender, args) => {
                    if (wrapSql) {
                        args.Query = this.sqlDialect.AddAffectedRowsCount(args.Query, command);
                    }
                };
                
                var returnsData = false;
                var enumeratedOperations = operations as IOperation[] ?? operations.ToArray();
                foreach (var operation in enumeratedOperations) {
                    wrapSql = IsRowsAffectedOperation(operation);
                    this.updateWriter.Write(operation, command);
                    if (wrapSql) {
                        returnsData = true;
                    }
                }

                command.WriteToDbCommand(dbCommand);
                if (!returnsData) {
                    await dbCommand.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
                    await transaction.CommitAsync(cancellationToken);
                    return;
                }

                await using (var dbReader = await dbCommand.ExecuteReaderAsync(cancellationToken).ConfigureAwait(false)) {
                    List<Exception> exceptions = null;
                    foreach (var operation in enumeratedOperations) {
                        var genericTypeDefinition = operation.GetType().GetGenericTypeDefinition();
                        if (genericTypeDefinition == typeof(UpdateOperation<,>) || genericTypeDefinition == typeof(DeleteOperation<>)) {
                            if (!await dbReader.ReadAsync(cancellationToken).ConfigureAwait(false)) {
                                AddException(ref exceptions, operation);
                            }

                            var affectedRows = await dbReader.GetFieldValueAsync<int>(0, cancellationToken);
                            if (affectedRows == 0) {
                                AddException(ref exceptions, operation);
                            }

                            await dbReader.NextResultAsync(cancellationToken).ConfigureAwait(false);
                        }
                    }

                    if (exceptions != null) {
                        throw new AggregateException(exceptions);
                    }
                }

                await transaction.CommitAsync(cancellationToken);
            }

            bool IsRowsAffectedOperation(IOperation operation) {
                var genericTypeDefinition = operation.GetType().GetGenericTypeDefinition();
                return genericTypeDefinition == typeof(UpdateOperation<,>) || genericTypeDefinition == typeof(DeleteOperation<>);
            }

            void AddException(ref List<Exception> exceptions, IOperation operation) {
                exceptions ??= new List<Exception>();
                exceptions.Add(new OptimisticConcurrencyException(operation));
            }
        }
    }
}