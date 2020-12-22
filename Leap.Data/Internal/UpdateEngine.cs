namespace Leap.Data.Internal {
    using System;
    using System.Data;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;

    using Leap.Data.Internal.UpdateWriter;
    using Leap.Data.Schema;
    using Leap.Data.UnitOfWork;

    class UpdateEngine : IAsyncDisposable {
        private readonly IConnectionFactory connectionFactory;

        private readonly ISchema schema;

        private readonly ISerializer serializer;

        private readonly ISqlUpdateWriter updateWriter;

        public UpdateEngine(IConnectionFactory connectionFactory, ISchema schema, ISerializer serializer, ISqlUpdateWriter updateWriter) {
            this.connectionFactory = connectionFactory;
            this.schema            = schema;
            this.serializer        = serializer;
            this.updateWriter      = updateWriter;
        }

        public async Task ExecuteAsync(UnitOfWork unitOfWork, CancellationToken cancellationToken = default) {
            if (!unitOfWork.Operations.Any()) {
                return;
            }

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
        }

        public ValueTask DisposeAsync() {
            throw new NotImplementedException();
        }
    }
}