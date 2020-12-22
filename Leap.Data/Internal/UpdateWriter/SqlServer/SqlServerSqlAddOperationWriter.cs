namespace Leap.Data.Internal.UpdateWriter.SqlServer {
    using Leap.Data.Internal.QueryWriter;
    using Leap.Data.Schema;

    class SqlServerSqlAddOperationWriter : SqlAddOperationWriter {
        public SqlServerSqlAddOperationWriter(ISchema schema, ISqlDialect sqlDialect, ISerializer serializer)
            : base(schema, sqlDialect, serializer) { }
    }
}