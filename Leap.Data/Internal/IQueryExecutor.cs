namespace Leap.Data.Internal {
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;

    using Leap.Data.Queries;

    public interface IQueryExecutor : IQueryGetter {
        ValueTask ExecuteAsync(IEnumerable<IQuery> queries, CancellationToken cancellationToken = default);

        ValueTask FlushAsync();
    }
}