namespace Leap.Data.Operations {
    using Fasterflect;

    using Leap.Data.Internal;
    using Leap.Data.Internal.ColumnValueFactories;
    using Leap.Data.Schema;

    static class OperationExtensions {
        public static bool IsAddOperation(this IOperation operation) {
            return operation.GetType().GetGenericTypeDefinition() == typeof(AddOperation<>);
        }

        public static bool IsUpdateOperation(this IOperation operation) {
            return operation.GetType().GetGenericTypeDefinition() == typeof(UpdateOperation<>);
        }

        public static bool IsDeleteOperation(this IOperation operation) {
            return operation.GetType().GetGenericTypeDefinition() == typeof(DeleteOperation<>);
        }

        public static object GetEntity(this IOperation operation) {
            return operation.GetPropertyValue(nameof(IOperation<string>.Entity));
        }

        public static DatabaseRow GetNewDatabaseRow(this IOperation operation, ISchema schema, ColumnValueFactoryFactory columnValueFactoryFactory) {
            return (DatabaseRow)typeof(OperationExtensions).CallMethod(
                operation.GetType().GenericTypeArguments,
                nameof(GetNewDatabaseRow),
                operation,
                schema,
                columnValueFactoryFactory);
        }

        public static DatabaseRow GetNewDatabaseRow<TEntity>(this IOperation<TEntity> operation, ISchema schema, ColumnValueFactoryFactory columnValueFactoryFactory) {
            return (DatabaseRow)typeof(OperationExtensions).CallMethod(
                new[] { typeof(TEntity), operation.Collection.KeyType },
                nameof(GetNewDatabaseRow),
                operation,
                columnValueFactoryFactory);
        }

        public static DatabaseRow GetNewDatabaseRow<TEntity, TKey>(this IOperation<TEntity> operation, ColumnValueFactoryFactory columnValueFactoryFactory) {
            var collection = operation.Collection;
            var key = collection.KeyExtractor.Extract<TEntity, TKey>(operation.Entity);
            var values = new object[collection.Columns.Count];
            foreach (var keyColumn in collection.KeyColumns) {
                values[collection.GetColumnIndex(keyColumn.Name)] = collection.KeyColumnValueExtractor.GetValue<TEntity, TKey>(keyColumn, key);
            }

            foreach (var nonKeyColumn in collection.NonKeyColumns) {
                values[collection.GetColumnIndex(nonKeyColumn.Name)] = columnValueFactoryFactory.GetFactory(nonKeyColumn).GetValue<TEntity, TKey>(nonKeyColumn, operation.Entity);
            }

            return new DatabaseRow(collection, values);
        }
    }
}