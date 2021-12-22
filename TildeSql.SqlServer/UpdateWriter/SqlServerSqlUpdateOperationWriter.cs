namespace TildeSql.SqlServer.UpdateWriter {
    using TildeSql.Internal.QueryWriter;
    using TildeSql.Internal.UpdateWriter;
    using TildeSql.Schema;
    using TildeSql.Serialization;

    public class SqlServerSqlUpdateOperationWriter : SqlUpdateOperationWriter {
        public SqlServerSqlUpdateOperationWriter(ISchema schema, ISqlDialect sqlDialect, ISerializer serializer)
            : base(schema, sqlDialect, serializer) { }
    }
}