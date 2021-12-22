namespace TildeSql.Internal {
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;

    using TildeSql.Queries;

    public interface IQueryExecutor {
        ValueTask ExecuteAsync(IEnumerable<IQuery> queries, CancellationToken cancellationToken = default);

        IAsyncEnumerable<object[]> GetAsync<TEntity>(IQuery query)
            where TEntity : class;

        ValueTask FlushAsync();
    }
}