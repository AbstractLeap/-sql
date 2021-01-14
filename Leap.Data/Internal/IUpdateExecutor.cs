namespace Leap.Data.Internal {
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;

    public interface IUpdateExecutor {
        ValueTask ExecuteAsync(
            IEnumerable<DatabaseRow> additions,
            IEnumerable<DatabaseRow> updates,
            IEnumerable<DatabaseRow> deletes,
            CancellationToken cancellationToken = default);
    }
}