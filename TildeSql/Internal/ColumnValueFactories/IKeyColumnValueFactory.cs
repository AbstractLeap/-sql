namespace TildeSql.Internal.ColumnValueFactories {
    using TildeSql.Schema.Columns;

    public interface IKeyColumnValueFactory : IColumnValueFactory {
        TValue GetValueUsingKey<TEntity, TKey, TValue>(Column column, TKey key);
    }
}