namespace Leap.Data.Internal {
    using System.Collections;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;

    using Leap.Data.IdentityMap;
    using Leap.Data.Queries;

    interface IQueryExecutor {
        ValueTask<ExecuteResult> ExecuteAsync(IEnumerable<IQuery> queries, CancellationToken cancellationToken = default);

        IAsyncEnumerable<Document<TEntity>> GetAsync<TEntity>(IQuery query) where TEntity : class;

        ValueTask FlushAsync();
    }
}