namespace Leap.Data.SqlServer.UpdateWriter {
    using Leap.Data.Internal.QueryWriter;
    using Leap.Data.Internal.UpdateWriter;
    using Leap.Data.Schema;
    using Leap.Data.Serialization;

    public class SqlServerSqlAddOperationWriter : SqlAddOperationWriter {
        public SqlServerSqlAddOperationWriter(ISchema schema, ISqlDialect sqlDialect, ISerializer serializer)
            : base(schema, sqlDialect, serializer) { }
    }
}