namespace Leap.Data.SqlServer {
    using System.Data.Common;

    using Leap.Data.Configuration.Sql;

    using Microsoft.Data.SqlClient;

    public class ConnectionPerCommandSqlServerConnectionFactory : ConnectionPerCommandConnectionFactory {
        public ConnectionPerCommandSqlServerConnectionFactory(string connectionString)
            : base(connectionString) { }

        protected override DbConnection CreateConnection(string connectionString) {
            return new SqlConnection(connectionString);
        }
    }
}