namespace Leap.Data.Schema {
    public interface ISchemaNamingSchemaConvention : ISchemaConvention {
        string GetSchemaName(string tableName);
    }
}