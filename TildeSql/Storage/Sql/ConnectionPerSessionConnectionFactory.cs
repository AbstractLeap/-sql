namespace TildeSql.Storage.Sql {
    using System;
    using System.Data.Common;
    using System.Threading.Tasks;

    public abstract class ConnectionPerSessionConnectionFactory : IConnectionFactory, IAsyncDisposable, IDisposable {
        private readonly string connectionString;

        private DbConnection connection;

        private readonly object connLock = new object();

        protected ConnectionPerSessionConnectionFactory(string connectionString) {
            this.connectionString = connectionString;
        }

        public DbConnection Get() {
            if (this.connection != null) return this.connection;
            lock (this.connLock) {
                this.connection ??= new ConnectionPerSessionDbConnectionWrapper(this.CreateConnection(this.connectionString));
            }

            return this.connection;
        }

        protected abstract DbConnection CreateConnection(string connectionString);

        public ValueTask DisposeAsync() {
            return this.connection?.DisposeAsync() ?? ValueTask.CompletedTask;
        }

        public void Dispose() {
            this.connection?.Dispose();
        }
    }
}