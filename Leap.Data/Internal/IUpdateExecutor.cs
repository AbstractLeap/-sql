namespace Leap.Data.Internal {
    using System.Threading;
    using System.Threading.Tasks;

    using Leap.Data.UnitOfWork;

    public interface IUpdateExecutor {
        ValueTask ExecuteAsync(UnitOfWork unitOfWork, CancellationToken cancellationToken = default);
    }
}