namespace TildeSql.SqlServer {
    public class ConnectionPerCommandSqlServerConnectionFactoryFactory : IConnectionFactoryFactory {
        private readonly string connectionString;

        public ConnectionPerCommandSqlServerConnectionFactoryFactory(string connectionString) {
            this.connectionString = connectionString;
        }

        public IConnectionFactory Get() {
            return new ConnectionPerCommandSqlServerConnectionFactory(this.connectionString);
        }
    }
}