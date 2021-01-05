namespace Leap.Data.SqlServer {
    using System.Data.Common;

    using Microsoft.Data.SqlClient;

    public class DefaultSqlServerConnectionFactory : IConnectionFactory {
        private readonly string connectionString;

        public DefaultSqlServerConnectionFactory(string connectionString) {
            this.connectionString = connectionString;
        }

        public DbConnection Get() {
            return new SqlConnection(this.connectionString);
        }
    }
}