namespace Leap.Data.Schema.Conventions.Sql {
    public interface ITableNamingSchemaConvention : ISchemaConvention {
        string GetTableName(string collectionName);
    }
}