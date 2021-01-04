namespace Leap.Data.Internal {
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;

    using Leap.Data.UnitOfWork;

    class UpdateEngine {
        private readonly IUpdateExecutor persistenceUpdateExecutor;

        public UpdateEngine(IUpdateExecutor persistenceUpdateExecutor) {
            this.persistenceUpdateExecutor = persistenceUpdateExecutor;
        }

        public async ValueTask ExecuteAsync(UnitOfWork unitOfWork, CancellationToken cancellationToken = default) {
            if (!unitOfWork.Operations.Any()) {
                return;
            }

            await this.persistenceUpdateExecutor.ExecuteAsync(unitOfWork, cancellationToken);
        }
    }
}