namespace Leap.Data.Internal.ColumnValueFactories {
    using Leap.Data.Schema.Columns;

    class NullColumnFactory : IColumnValueFactory {
        public TValue GetValue<TEntity, TKey, TValue>(Column column, TEntity entity) {
            return default;
        }
    }
}