namespace TildeSql.SqlServer.QueryWriter {
    using TildeSql.Internal.QueryWriter;
    using TildeSql.Schema;

    public class SqlServerSqlEntityQueryWriter : SqlEntityQueryWriter {
        public SqlServerSqlEntityQueryWriter(ISchema schema)
            : base(schema, new SqlServerDialect()) { }
    }
}