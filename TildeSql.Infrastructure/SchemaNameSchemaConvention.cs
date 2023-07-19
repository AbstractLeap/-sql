namespace TildeSql.Infrastructure {
    using TildeSql.Schema.Conventions.Sql;

    public class SchemaNameSchemaConvention : ISchemaNamingSchemaConvention {
        public string GetSchemaName(string tableName) {
            return "dbo";
        }
    }
}