namespace TildeSql.SqlServer {
    public class SqlServerConfiguration {
        public IConnectionFactoryFactory ConnectionFactoryFactory { get; set; }
        
        public ConnectionMode? ConnectionMode { get; set; }
    }

    public enum ConnectionMode {
        PerCommand,
        PerSession
    }
}