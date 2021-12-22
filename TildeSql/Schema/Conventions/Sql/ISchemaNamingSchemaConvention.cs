namespace TildeSql.Schema.Conventions.Sql {
    public interface ISchemaNamingSchemaConvention : ISchemaConvention {
        string GetSchemaName(string tableName);
    }
}