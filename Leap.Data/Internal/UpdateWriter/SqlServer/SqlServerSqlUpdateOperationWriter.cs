namespace Leap.Data.Internal.UpdateWriter.SqlServer {
    using Leap.Data.Internal.QueryWriter;
    using Leap.Data.Schema;

    class SqlServerSqlUpdateOperationWriter : SqlUpdateOperationWriter {
        public SqlServerSqlUpdateOperationWriter(ISchema schema, ISqlDialect sqlDialect, ISerializer serializer)
            : base(schema, sqlDialect, serializer) { }
    }
}