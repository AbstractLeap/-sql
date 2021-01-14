namespace Leap.Data.SqlServer.UpdateWriter {
    using Leap.Data.Internal;
    using Leap.Data.Internal.UpdateWriter;
    using Leap.Data.Operations;
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

        public void Write(IOperation operation, Command command) {
            if (operation.IsAddOperation()) {
                this.addOperationWriter.Write(operation, command);
            }
            else if (operation.IsUpdateOperation()) {
                this.updateOperationWriter.Write(operation, command);
            }
            else if (operation.IsDeleteOperation()) {
                this.deleteOperationWriter.Write(operation, command);
            }
        }
    }
}