namespace TildeSql.Internal.ColumnValueFactories {
    using Fasterflect;

    using TildeSql.Schema.Columns;

    static class ColumnValueFactoryExtensions {
        public static object GetValue<TEntity, TKey>(this IColumnValueFactory columnValueFactory, Column column, TEntity entity) {
            return columnValueFactory.CallMethod(
                new[] { typeof(TEntity), typeof(TKey), column.Type },
                nameof(IColumnValueFactory.GetValue),
                new[] { typeof(Column), typeof(TEntity) },
                column,
                entity);
        }

        public static object GetValue(this IColumnValueFactory columnValueFactory, Column column, object entity) {
            return columnValueFactory.CallMethod(
                new[] { column.Collection.BaseEntityType, column.Collection.KeyType, column.Type },
                nameof(IColumnValueFactory.GetValue),
                new[] { typeof(Column), column.Collection.BaseEntityType },
                column,
                entity);
        }
    }
}