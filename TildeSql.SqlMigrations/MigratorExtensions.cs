namespace TildeSql.SqlMigrations {
    using System;

    using FluentMigrator;
    using FluentMigrator.Builders;
    using FluentMigrator.Infrastructure;

    public static class MigratorExtensions {
        public static bool SupportsJson { get; set; } = false;

        public static void ComputedColumn(this Migration up, string columnName, string tableName, string schemaName, string computedColumnExpression, bool persisted) {
            up.Execute.Sql($@"ALTER TABLE [{schemaName}].[{tableName}] ADD [{columnName}] AS ({computedColumnExpression}){(persisted ? " PERSISTED;" : "; ")}");
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
    }
}