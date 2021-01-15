namespace Leap.Data.SqlServer {
    using System.Data.Common;

    using Microsoft.Data.SqlClient;

    public class ConnectionPerCommandSqlServerConnectionFactory : IConnectionFactory {
        private readonly string connectionString;

        public ConnectionPerCommandSqlServerConnectionFactory(string connectionString) {
            this.connectionString = connectionString;
        }

        public DbConnection Get() {
            return new SqlConnection(this.connectionString);
        }
    }
}