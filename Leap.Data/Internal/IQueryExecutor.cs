namespace Leap.Data.Internal {
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;

    using Leap.Data.Queries;

    public interface IQueryExecutor {
        ValueTask ExecuteAsync(IEnumerable<IQuery> queries, CancellationToken cancellationToken = default);

        IAsyncEnumerable<object[]> GetAsync<TEntity>(IQuery query)
            where TEntity : class;

        ValueTask FlushAsync();
    }
}