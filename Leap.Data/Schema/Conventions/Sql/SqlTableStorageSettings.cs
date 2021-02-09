namespace Leap.Data.Schema.Conventions.Sql {
    public class SqlTableStorageSettings : ITableStorageSettings {
        public string TableName { get; init; }
        
        public string SchemaName { get; init; }
    }
}