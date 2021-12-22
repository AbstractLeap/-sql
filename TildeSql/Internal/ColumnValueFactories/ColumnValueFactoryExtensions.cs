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
    }
}