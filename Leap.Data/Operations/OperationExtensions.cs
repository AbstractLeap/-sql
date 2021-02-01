﻿namespace Leap.Data.Operations {
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
                new[] { typeof(TEntity), operation.Table.KeyType },
                nameof(GetNewDatabaseRow),
                operation,
                columnValueFactoryFactory);
        }

        public static DatabaseRow GetNewDatabaseRow<TEntity, TKey>(this IOperation<TEntity> operation, ColumnValueFactoryFactory columnValueFactoryFactory) {
            var table = operation.Table;
            var key = table.KeyExtractor.Extract<TEntity, TKey>(operation.Entity);
            var values = new object[table.Columns.Count];
            foreach (var keyColumn in table.KeyColumns) {
                values[table.GetColumnIndex(keyColumn.Name)] = table.KeyColumnValueExtractor.GetValue<TEntity, TKey>(keyColumn, key);
            }

            foreach (var nonKeyColumn in table.NonKeyColumns) {
                values[table.GetColumnIndex(nonKeyColumn.Name)] = columnValueFactoryFactory.GetFactory(nonKeyColumn).GetValue<TEntity, TKey>(nonKeyColumn, operation.Entity);
            }

            return new DatabaseRow(table, values);
        }
    }
}