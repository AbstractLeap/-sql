namespace TildeSql.Internal.Caching {
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;

    using TildeSql.Queries;

    internal interface ICacheExecutor {
        ValueTask<ExecuteResult> ExecuteAsync(IList<IQuery> queries, CancellationToken cancellationToken = default);

        IAsyncEnumerable<object[]> GetAsync<TEntity>(IQuery query)
            where TEntity : class;
    }
}