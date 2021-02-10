namespace Leap.Data.Schema.Conventions.Sql {
    public class SqlCollectionStorageSettings : ICollectionStorageSettings {
        public string TableName { get; init; }
        
        public string SchemaName { get; init; }
    }
}