namespace TildeSql {
    using System;
    using System.Threading;
    using System.Threading.Tasks;

    public interface ISession : IAsyncDisposable, IDisposable {
        IQueryBuilder<TEntity> Get<TEntity>()
            where TEntity : class;

        IQueryBuilder<TEntity> Get<TEntity>(string collectionName)
            where TEntity : class;

        Task SaveChangesAsync(CancellationToken cancellationToken = default);

        void DisableTracking();

        void EnableTracking();

        void Delete<TEntity>(TEntity entity)
            where TEntity : class;

        void Delete<TEntity>(TEntity entity, string collectionName)
            where TEntity : class;

        void Add<TEntity>(TEntity entity)
            where TEntity : class;

        void Add<TEntity>(TEntity entity, string collectionName)
            where TEntity : class;

        IEntityInspector<TEntity> Inspect<TEntity>(TEntity entity)
            where TEntity : class;

        IEntityInspector<TEntity> Inspect<TEntity>(TEntity entity, string collectionName)
            where TEntity : class;
    }
}