namespace Leap.Data.SqlServer {
    using System;
    using System.Data.Common;
    using System.Threading.Tasks;

    using Microsoft.Data.SqlClient;

    public class ConnectionPerSessionSqlServerConnectionFactory : IConnectionFactory, IAsyncDisposable {
        private readonly string connectionString;

        private DbConnection connection;

        private readonly object connLock = new object();

        public ConnectionPerSessionSqlServerConnectionFactory(string connectionString) {
            this.connectionString = connectionString;
        }

        public DbConnection Get() {
            if (this.connection == null) {
                lock (this.connLock) {
                    if (this.connection == null) {
                        this.connection = new ConnectionPerSessionDbConnectionWrapper(new SqlConnection(this.connectionString));
                    }
                }
            }

            return this.connection;
        }

        public ValueTask DisposeAsync() {
            if (this.connection != null) {
                return this.connection.DisposeAsync();
            }

            return ValueTask.CompletedTask;
        }
    }
}