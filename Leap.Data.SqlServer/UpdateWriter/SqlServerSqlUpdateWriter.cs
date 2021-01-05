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
            var genericTypeDefinition = operation.GetType().GetGenericTypeDefinition();
            if (genericTypeDefinition == typeof(AddOperation<>)) {
                this.addOperationWriter.Write(operation, command);
            }
            else if (genericTypeDefinition == typeof(UpdateOperation<,>)) {
                this.updateOperationWriter.Write(operation, command);
            }
            else if (genericTypeDefinition == typeof(DeleteOperation<>)) {
                this.deleteOperationWriter.Write(operation, command);
            }
        }
    }
}