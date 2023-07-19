namespace TildeSql.Model {
    using System.Collections.Generic;

    public interface IRepository<TEntity, TKey>
        where TEntity : class {
        ISingleEntityAccessor<TEntity, TKey> GetByIdAsync(TKey key, CancellationToken cancellationToken = default);

        IAsyncEnumerable<TEntity> All();

        void Add(TEntity entity);

        void Remove(TEntity entity);
    }
}
