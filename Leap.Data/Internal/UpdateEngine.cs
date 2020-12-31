namespace Leap.Data.Internal
{
    using System;
    using System.Data;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;

    using Leap.Data.Internal.UpdateWriter;
    using Leap.Data.Schema;
    using Leap.Data.UnitOfWork;

    class UpdateEngine {
        
        private readonly ISchema schema;

        private readonly ISerializer serializer;

        private readonly SqlUpdateExecutor queryExecutor;

        public UpdateEngine(IConnectionFactory connectionFactory, ISchema schema, ISerializer serializer, ISqlUpdateWriter updateWriter) {
            this.queryExecutor = new SqlUpdateExecutor(connectionFactory, updateWriter);
        }

        public async ValueTask ExecuteAsync(UnitOfWork unitOfWork, CancellationToken cancellationToken = default) {
            if (!unitOfWork.Operations.Any()) {
                return;
            }

            await this.queryExecutor.ExecuteAsync(unitOfWork, cancellationToken);
        }
    }
}