namespace TildeSql.SqlServer.UpdateWriter {
    using TildeSql.Internal.QueryWriter;
    using TildeSql.Internal.UpdateWriter;
    using TildeSql.Schema;
    using TildeSql.Serialization;

    public class SqlServerSqlAddOperationWriter : SqlAddOperationWriter {
        public SqlServerSqlAddOperationWriter(ISchema schema, ISqlDialect sqlDialect, ISerializer serializer)
            : base(schema, sqlDialect, serializer) { }
    }
}