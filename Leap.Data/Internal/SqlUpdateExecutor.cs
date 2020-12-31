namespace Leap.Data.Internal
{
    using System.Threading;

    class SqlUpdateExecutor : IUpdateExecutor {
        private readonly IConnectionFactory connectionFactory;

        private readonly ISqlUpdateWriter updateWriter;

        public SqlUpdateExecutor(IConnectionFactory connectionFactory, ISqlUpdateWriter updateWriter) {
            this.connectionFactory = connectionFactory;
            this.updateWriter = updateWriter;
        }

        public ValueTask ExecuteAsync(UnitOfWork unitOfWork, CancellationToken cancellationToken = default) {
            var connection = this.connectionFactory.Get();
            if (connection.State != ConnectionState.Open) {
                await connection.OpenAsync(cancellationToken).ConfigureAwait(false);
            }

            var dbCommand = connection.CreateCommand();
            var command = new Command();
            foreach (var operation in unitOfWork.Operations) {
                this.updateWriter.Write(operation, command);
            }

            command.WriteToDbCommand(dbCommand);
            await dbCommand.ExecuteNonQueryAsync(cancellationToken);
            // TODO cleanup resources
        }
    }
}