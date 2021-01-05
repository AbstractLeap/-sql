namespace Leap.Data.SqlServer.QueryWriter {
    using Leap.Data.Internal.QueryWriter;
    using Leap.Data.Schema;

    public class SqlServerSqlEntityQueryWriter : SqlEntityQueryWriter {
        public SqlServerSqlEntityQueryWriter(ISchema schema)
            : base(schema, new SqlServerDialect()) { }
    }
}