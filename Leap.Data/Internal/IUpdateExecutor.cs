namespace Leap.Data.Internal
{
    using System.Threading;

    interface IUpdateExecutor {
        ValueTask ExecuteAsync(UnitOfWork unitOfWork, CancellationToken cancellationToken = default);
    }
}