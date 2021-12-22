namespace TildeSql.Internal.ColumnValueFactories {
    using TildeSql.Schema.Columns;

    class NullColumnFactory : IColumnValueFactory {
        public TValue GetValue<TEntity, TKey, TValue>(Column column, TEntity entity) {
            return default;
        }
    }
}