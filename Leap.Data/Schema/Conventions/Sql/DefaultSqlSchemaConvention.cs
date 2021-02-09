namespace Leap.Data.Schema.Conventions.Sql {
    public class DefaultSqlSchemaConvention : ITableNamingSchemaConvention, ISchemaNamingSchemaConvention {
        public virtual string GetTableName(string collectionName) => collectionName;

        public virtual string GetSchemaName(string tableName) => "dbo";
    }
}