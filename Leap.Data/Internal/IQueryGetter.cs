namespace Leap.Data.Internal {
    using System.Collections.Generic;

    using Leap.Data.IdentityMap;
    using Leap.Data.Queries;

    public interface IQueryGetter {
        IAsyncEnumerable<Document<TEntity>> GetAsync<TEntity>(IQuery query)
            where TEntity : class;
    }
}