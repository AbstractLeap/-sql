namespace TildeSql {
    using System;
    using System.Collections.Generic;

    public interface IEntityQueryBuilder<TEntity> : IAsyncEnumerable<TEntity> {
        IEntityQueryBuilder<TEntity> Where(string whereClause, IDictionary<string, object> parameters = null);
        
        IEntityQueryBuilder<TEntity> Where(string whereClause, object parameters);

        IEntityQueryBuilder<TEntity> Join(string joinClause);

        IEntityQueryBuilder<TEntity> OrderBy(string orderByClause);

        IEntityQueryBuilder<TEntity> Offset(int offset);

        IEntityQueryBuilder<TEntity> Limit(int limit);

        IEntityQueryBuilder<TEntity> Limit(int limit, out ICountAccessor countAccessor);

        IEntityQueryBuilder<TEntity> Cache(TimeSpan? absoluteExpirationRelativeToNow = null, string key = null);

        IEntityQueryBuilder<TEntity> NoCache();

        IFutureEntityQueryResult<TEntity> Future();
    }
}