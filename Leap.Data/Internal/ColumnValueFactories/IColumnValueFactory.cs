namespace Leap.Data.Internal.ColumnValueFactories {
    using Leap.Data.IdentityMap;
    using Leap.Data.Schema.Columns;

    public interface IColumnValueFactory {
        TValue GetValue<TEntity, TKey, TValue>(Column column, TEntity entity, IDocument<TEntity> document);
    }
}