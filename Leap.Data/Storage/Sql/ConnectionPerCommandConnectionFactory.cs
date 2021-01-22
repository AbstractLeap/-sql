namespace Leap.Data.Storage.Sql {
    using System.Data.Common;

    public abstract class ConnectionPerCommandConnectionFactory : IConnectionFactory {
        private readonly string connectionString;

        protected ConnectionPerCommandConnectionFactory(string connectionString) {
            this.connectionString = connectionString;
        }

        public DbConnection Get() {
            return this.CreateConnection(this.connectionString);
        }

        protected abstract DbConnection CreateConnection(string connectionString);
    }
}