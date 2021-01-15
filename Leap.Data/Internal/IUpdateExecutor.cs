namespace Leap.Data.Internal {
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;

    public interface IUpdateExecutor {
        ValueTask ExecuteAsync(
            IEnumerable<DatabaseRow> inserts,
            IEnumerable<(DatabaseRow OldDatabaseRow, DatabaseRow NewDatabaseRow)> updates,
            IEnumerable<DatabaseRow> deletes,
            CancellationToken cancellationToken = default);
    }
}