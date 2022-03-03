namespace TildeSql.Internal.ColumnValueFactories {
    using TildeSql.Schema.Columns;

    class DocumentTypeColumnValueFactory : IColumnValueFactory {
        public TValue GetValue<TEntity, TKey, TValue>(Column column, TEntity entity) {
            return (TValue)(object)column.Collection.TypeSerializer.Serialize(entity.GetType());
        }
    }
}