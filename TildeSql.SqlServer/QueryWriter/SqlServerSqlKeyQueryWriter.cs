namespace TildeSql.SqlServer.QueryWriter {
    using TildeSql.Internal.QueryWriter;
    using TildeSql.Schema;

    public class SqlServerSqlKeyQueryWriter : SqlKeyQueryWriter {
        public SqlServerSqlKeyQueryWriter(ISchema schema)
            : base(schema, new SqlServerDialect()) { }
    }
}