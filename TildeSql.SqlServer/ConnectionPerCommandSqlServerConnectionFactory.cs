namespace TildeSql.SqlServer {
    using System.Data.Common;

    using Microsoft.Data.SqlClient;

    using TildeSql.Storage.Sql;

    public class ConnectionPerCommandSqlServerConnectionFactory : ConnectionPerCommandConnectionFactory {
        public ConnectionPerCommandSqlServerConnectionFactory(string connectionString)
            : base(connectionString) { }

        protected override DbConnection CreateConnection(string connectionString) {
            return new SqlConnection(connectionString);
        }
    }
}