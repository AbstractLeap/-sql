namespace Leap.Data.SqlServer {
    using System.Data;
    using System.Data.Common;
    using System.Threading.Tasks;

    using Microsoft.Data.SqlClient;

    /// <summary>
    /// This class overrides the dispose functionality so that the disposal calls inside the session are ignored until the whole session is disposed
    /// </summary>
    public class ConnectionPerSessionDbConnectionWrapper : DbConnection {
        private readonly DbConnection dbConnectionImplementation;

        public ConnectionPerSessionDbConnectionWrapper(SqlConnection sqlConnection) {
            this.dbConnectionImplementation = sqlConnection;
        }

        protected override DbTransaction BeginDbTransaction(IsolationLevel isolationLevel) {
            return this.dbConnectionImplementation.BeginTransaction(isolationLevel);
        }

        public override void ChangeDatabase(string databaseName) {
            this.dbConnectionImplementation.ChangeDatabase(databaseName);
        }

        public override void Close() {
            this.dbConnectionImplementation.Close();
        }

        public override void Open() {
            this.dbConnectionImplementation.Open();
        }

        public override string ConnectionString {
            get => this.dbConnectionImplementation.ConnectionString;
            set => this.dbConnectionImplementation.ConnectionString = value;
        }

        public override string Database => this.dbConnectionImplementation.Database;

        public override ConnectionState State => this.dbConnectionImplementation.State;

        public override string DataSource => this.dbConnectionImplementation.DataSource;

        public override string ServerVersion => this.dbConnectionImplementation.ServerVersion;

        protected override DbCommand CreateDbCommand() {
            return this.dbConnectionImplementation.CreateCommand();
        }

        protected override void Dispose(bool disposing) {
            // the connection factory manages disposal
        }

        public override ValueTask DisposeAsync() {
            return ValueTask.CompletedTask;
        }
    }
}