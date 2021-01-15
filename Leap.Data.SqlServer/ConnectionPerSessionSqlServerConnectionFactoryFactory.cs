namespace Leap.Data.SqlServer {
    public class ConnectionPerSessionSqlServerConnectionFactoryFactory : IConnectionFactoryFactory
    {
        private readonly string connectionString;

        public ConnectionPerSessionSqlServerConnectionFactoryFactory(string connectionString)
        {
            this.connectionString = connectionString;
        }

        public IConnectionFactory Get()
        {
            return new ConnectionPerSessionSqlServerConnectionFactory(this.connectionString);
        }
    }
}