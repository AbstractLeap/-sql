namespace TildeSql.SqlMigrations {
    using System;

    using FluentMigrator;
    using FluentMigrator.Builders;
    using FluentMigrator.Infrastructure;

    public static class MigratorExtensions {
        public static bool SupportsJson { get; set; } = false;

        public static bool SupportsVectors { get; set; } = false;

        public static void ComputedColumn(
            this Migration up,
            string columnName,
            string tableName,
            string schemaName,
            string computedColumnExpression,
            bool persisted,
            string collation = null) {
            up.Execute.Sql(
                $@"ALTER TABLE [{schemaName}].[{tableName}] ADD [{columnName}] AS ({computedColumnExpression}){(persisted ? " PERSISTED" : string.Empty)}{(!string.IsNullOrWhiteSpace(collation) ? " COLLATE " + collation : string.Empty)};");
        }

        public static TNext AsJson<TNext>(this IColumnTypeSyntax<TNext> column)
            where TNext : IFluentSyntax {
            if (SupportsJson) {
                column.AsCustom("json");
            }
            else {
                column.AsString(Int32.MaxValue);
            }

            return (TNext)column;
        }

        public static TNext AsVector<TNext>(this IColumnTypeSyntax<TNext> column, int dimensions)
            where TNext : IFluentSyntax {
            if (SupportsVectors) {
                column.AsCustom($"vector({dimensions})");
            }
            else {
                column.AsString(Int32.MaxValue);
            }

            return (TNext)column;
        }
    }
}