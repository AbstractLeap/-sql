namespace TildeSql.Internal.ColumnValueFactories {
    using System;

    using TildeSql.Schema.Columns;

    class ProjectionColumnFactory : IColumnValueFactory {
        public TValue GetValue<TEntity, TKey, TValue>(Column column, TEntity entity) {
            if (!(column is ProjectionColumn<TEntity, TValue> projectionColumn)) {
                if (column.Collection.BaseEntityType == typeof(TEntity)) {
                    throw new Exception("Unable to get projection column type");
                }

                // try using the base entity type for the collection
                return (TValue)this.GetValue(column, entity);
            }

            return projectionColumn.ProjectionFunc(entity);
        }
    }
}