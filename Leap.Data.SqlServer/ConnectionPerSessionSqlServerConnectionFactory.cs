namespace Leap.Data.SqlServer {
    using System.Data.Common;

    using Leap.Data.Storage.Sql;

    using Microsoft.Data.SqlClient;

    public class ConnectionPerSessionSqlServerConnectionFactory : ConnectionPerSessionConnectionFactory {
        public ConnectionPerSessionSqlServerConnectionFactory(string connectionString)
            : base(connectionString) { }

        protected override DbConnection CreateConnection(string connectionString) {
            return new SqlConnection(connectionString);
        }
    }
}