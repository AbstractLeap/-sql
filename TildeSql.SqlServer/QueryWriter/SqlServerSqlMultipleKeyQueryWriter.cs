namespace TildeSql.SqlServer.QueryWriter {
    using TildeSql.Internal.QueryWriter;
    using TildeSql.Schema;

    public class SqlServerSqlMultipleKeyQueryWriter : SqlMultipleKeyQueryWriter {
        public SqlServerSqlMultipleKeyQueryWriter(ISchema schema)
            : base(schema, new SqlServerDialect()) { }
    }
}