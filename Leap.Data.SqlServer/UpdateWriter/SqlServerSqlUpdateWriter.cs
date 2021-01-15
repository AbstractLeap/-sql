namespace Leap.Data.SqlServer.UpdateWriter {
    using Leap.Data.Internal;
    using Leap.Data.Internal.UpdateWriter;
    using Leap.Data.Schema;
    using Leap.Data.Serialization;

    public class SqlServerSqlUpdateWriter : ISqlUpdateWriter {
        private readonly SqlServerSqlAddOperationWriter addOperationWriter;

        private readonly SqlServerSqlUpdateOperationWriter updateOperationWriter;

        private readonly SqlServerSqlDeleteOperationWriter deleteOperationWriter;

        public SqlServerSqlUpdateWriter(ISchema schema, ISerializer serializer) {
            this.addOperationWriter    = new SqlServerSqlAddOperationWriter(schema, new SqlServerDialect(), serializer);
            this.updateOperationWriter = new SqlServerSqlUpdateOperationWriter(schema, new SqlServerDialect(), serializer);
            this.deleteOperationWriter = new SqlServerSqlDeleteOperationWriter(schema, new SqlServerDialect(), serializer);
        }

        public void WriteInsert(DatabaseRow row, Command command) {
            this.addOperationWriter.Write(row, command);
        }

        public void WriteUpdate((DatabaseRow OldDatabaseRow, DatabaseRow NewDatabaseRow) row, Command command) {
            this.updateOperationWriter.Write(row, command);
        }

        public void WriteDelete(DatabaseRow row, Command command) {
            this.deleteOperationWriter.Write(row, command);
        }
    }
}