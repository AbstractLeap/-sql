namespace TildeSql.SqlMigrations {
    using FluentMigrator;

    public static class MigratorExtensions {
        public static void ComputedColumn(this Migration up, string columnName, string tableName, string schemaName, string computedColumnExpression, bool persisted) {
            up.Execute.Sql($@"ALTER TABLE [{schemaName}].[{tableName}] ADD [{columnName}] AS ({computedColumnExpression}){(persisted ? " PERSISTED;" : "; ")}");
        }
    }
}