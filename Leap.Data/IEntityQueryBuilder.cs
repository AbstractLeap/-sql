namespace Leap.Data {
    using System.Collections.Generic;

    public interface IEntityQueryBuilder<TEntity> : IAsyncEnumerable<TEntity> {
        IEntityQueryBuilder<TEntity> Where(string whereClause);

        IEntityQueryBuilder<TEntity> OrderBy(string orderByClause);

        IEntityQueryBuilder<TEntity> Offset(int offset);

        IEntityQueryBuilder<TEntity> Limit(int limit);

        IFutureEntityQueryResult<TEntity> Future();
    }
}