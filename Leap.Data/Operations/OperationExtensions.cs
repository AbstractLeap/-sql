namespace Leap.Data.Operations {
    using Fasterflect;

    using Leap.Data.IdentityMap;
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

        public static void UpdateDocument(this IOperation operation, DatabaseRow databaseRow, DocumentState state) {
            typeof(OperationExtensions).CallMethod(operation.GetType().GenericTypeArguments, nameof(SetDatabaseRow), operation, databaseRow, state);
        }

        public static void SetDatabaseRow<TEntity>(this IOperation<TEntity> operation, DatabaseRow databaseRow, DocumentState state) {
            operation.Document.Row   = databaseRow;
            operation.Document.State = state;
        }

        public static DatabaseRow GetCurrentDatabaseRow(this IOperation operation) {
            return (DatabaseRow)typeof(OperationExtensions).CallMethod(operation.GetType().GenericTypeArguments, nameof(GetCurrentDatabaseRow), operation);
        }

        public static DatabaseRow GetCurrentDatabaseRow<TEntity>(this IOperation<TEntity> operation) {
            return operation.Document.Row;
        }

        public static DatabaseRow GetNewDatabaseRow(this IOperation operation, ISchema schema, ColumnValueFactoryFactory columnValueFactoryFactory) {
            return (DatabaseRow)typeof(OperationExtensions).CallMethod(operation.GetType().GenericTypeArguments, nameof(GetNewDatabaseRow), operation, schema, columnValueFactoryFactory);
        }

        public static DatabaseRow GetNewDatabaseRow<TEntity>(this IOperation<TEntity> operation, ISchema schema, ColumnValueFactoryFactory columnValueFactoryFactory) {
            var table = schema.GetTable<TEntity>();
            return (DatabaseRow)typeof(OperationExtensions).CallMethod(
                new[] { typeof(TEntity), table.KeyType },
                nameof(GetNewDatabaseRow),
                operation,
                table,
                columnValueFactoryFactory);
        }

        public static DatabaseRow GetNewDatabaseRow<TEntity, TKey>(this IOperation<TEntity> operation, Table table, ColumnValueFactoryFactory columnValueFactoryFactory) {
            var key = table.KeyExtractor.Extract<TEntity, TKey>(operation.Document.Entity);
            var values = new object[table.Columns.Count];
            foreach (var keyColumn in table.KeyColumns) {
                values[table.GetColumnIndex(keyColumn.Name)] = table.KeyColumnValueExtractor.GetValue<TEntity, TKey>(keyColumn, key);
            }
            
            foreach (var nonKeyColumn in table.NonKeyColumns) {
                values[table.GetColumnIndex(nonKeyColumn.Name)] = columnValueFactoryFactory.GetFactory(nonKeyColumn)
                                                                                           .GetValue<TEntity, TKey>(nonKeyColumn, operation.Document.Entity, operation.Document);
            }

            return new DatabaseRow(table, values);
        }
    }
}