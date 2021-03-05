namespace Leap.Data.Operations {
    using Fasterflect;

    using Leap.Data.Internal;
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

        public static DatabaseRow GetNewDatabaseRow(this IOperation operation, ISchema schema, DatabaseRowFactory databaseRowFactory) {
            return (DatabaseRow)typeof(OperationExtensions).CallMethod(
                operation.GetType().GenericTypeArguments,
                nameof(GetNewDatabaseRow),
                operation,
                schema,
                databaseRowFactory);
        }

        public static DatabaseRow GetNewDatabaseRow<TEntity>(this IOperation<TEntity> operation, ISchema schema, DatabaseRowFactory databaseRowFactory) {
            return (DatabaseRow)typeof(OperationExtensions).CallMethod(
                new[] { typeof(TEntity), operation.Collection.KeyType },
                nameof(GetNewDatabaseRow),
                operation,
                databaseRowFactory);
        }

        public static DatabaseRow GetNewDatabaseRow<TEntity, TKey>(this IOperation<TEntity> operation, DatabaseRowFactory databaseRowFactory) {
            return databaseRowFactory.Create<TEntity, TKey>(operation.Collection, operation.Entity);
        }
    }
}