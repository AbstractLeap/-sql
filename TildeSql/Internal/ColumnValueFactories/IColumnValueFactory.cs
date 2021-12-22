namespace TildeSql.Internal.ColumnValueFactories {
    using TildeSql.Schema.Columns;

    public interface IColumnValueFactory {
        TValue GetValue<TEntity, TKey, TValue>(Column column, TEntity entity);
    }
}