namespace Leap.Data {
    using System.Threading;
    using System.Threading.Tasks;

    public interface ISession {
        //Task<TEntity> LoadAsync<TEntity, TKey>(TKey key, CancellationToken cancellationToken = default);

        //IAsyncEnumerable<TEntity> LoadManyAsync<TEntity>(CancellationToken cancellationToken = default);

        //IAsyncEnumerable<TEntity> LoadManyAsync<TEntity>(Query<TEntity> query, CancellationToken cancellationToken = default);

        //void AddFutureQuery<TEntity>(Query<TEntity> query);

        IQueryBuilder<TEntity> Get<TEntity>() where TEntity : class;

        Task SaveChangesAsync(CancellationToken cancellationToken = default);

        void Delete<TEntity>(TEntity entity) where TEntity : class;

        void Add<TEntity>(TEntity entity) where TEntity : class;
    }

    //public class QueryBuilder<T> : IQueryBuilder<T> {
    //    private readonly Query<T> query = new Query<T>();

    //    public QueryBuilder<T> Where(string whereClause) {
    //        query.WhereClause = whereClause;
    //        return this;
    //    }

    //    public QueryBuilder<T> OrderBy(string orderByClause) {
    //        query.OrderByClause = orderByClause;
    //        return this;
    //    }

    //    public QueryBuilder<T> Offset(int offset) {
    //        query.Offset = offset;
    //        return this;
    //    }

    //    public QueryBuilder<T> Limit(int limit) {
    //        query.Limit = limit;
    //        return this;
    //    }

    //    public Query<T> Build() {
    //        return query;
    //    }
    //}
}