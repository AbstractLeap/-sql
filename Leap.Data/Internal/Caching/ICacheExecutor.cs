namespace Leap.Data.Internal.Caching {
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;

    using Leap.Data.Queries;

    internal interface ICacheExecutor {
        ValueTask<ExecuteResult> ExecuteAsync(IEnumerable<IQuery> queries, CancellationToken cancellationToken = default);

        IAsyncEnumerable<object[]> GetAsync<TEntity>(IQuery query)
            where TEntity : class;
    }
}