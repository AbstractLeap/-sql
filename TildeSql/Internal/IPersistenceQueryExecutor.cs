namespace TildeSql.Internal {
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;

    using TildeSql.Queries;

    public interface IPersistenceQueryExecutor {
        ValueTask ExecuteAsync(IEnumerable<IQuery> queries, CancellationToken cancellationToken = default);

        IAsyncEnumerable<object[]> GetAsync(IQuery query);

        ValueTask FlushAsync();
    }
}