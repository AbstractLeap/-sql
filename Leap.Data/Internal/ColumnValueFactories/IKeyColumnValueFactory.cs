namespace Leap.Data.Internal.ColumnValueFactories {
    using Leap.Data.Schema.Columns;

    public interface IKeyColumnValueFactory : IColumnValueFactory {
        TValue GetValue<TEntity, TKey, TValue>(Column column, TKey key);
    }
}