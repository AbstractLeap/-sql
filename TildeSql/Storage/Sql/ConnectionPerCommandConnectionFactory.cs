namespace TildeSql.Storage.Sql {
    using System.Data.Common;
    using System.Threading.Tasks;

    public abstract class ConnectionPerCommandConnectionFactory : IConnectionFactory {
        private readonly string connectionString;

        protected ConnectionPerCommandConnectionFactory(string connectionString) {
            this.connectionString = connectionString;
        }

        public ValueTask<DbConnection> GetAsync() {
            return ValueTask.FromResult(this.CreateConnection(this.connectionString));
        }

        protected abstract DbConnection CreateConnection(string connectionString);
    }
}