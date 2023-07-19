namespace TildeSql.Model {
    public interface ISingleEntityAccessor<TEntity, TKey> {
        ValueTask<TEntity> SingleOrDefaultAsync();
    }
}