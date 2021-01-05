namespace Leap.Data {
    using System.Threading;
    using System.Threading.Tasks;

    public interface ISession {
        IQueryBuilder<TEntity> Get<TEntity>()
            where TEntity : class;

        Task SaveChangesAsync(CancellationToken cancellationToken = default);

        void Delete<TEntity>(TEntity entity)
            where TEntity : class;

        void Add<TEntity>(TEntity entity)
            where TEntity : class;
    }
}