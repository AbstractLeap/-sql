namespace Leap.Data.Internal {
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;

    using Leap.Data.Operations;

    public interface IUpdateExecutor {
        ValueTask ExecuteAsync(IEnumerable<IOperation> operations, CancellationToken cancellationToken = default);
    }
}