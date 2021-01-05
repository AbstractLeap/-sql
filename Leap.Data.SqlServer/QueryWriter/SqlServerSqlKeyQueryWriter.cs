namespace Leap.Data.SqlServer.QueryWriter {
    using Leap.Data.Internal.QueryWriter;
    using Leap.Data.Schema;

    public class SqlServerSqlKeyQueryWriter : SqlKeyQueryWriter {
        public SqlServerSqlKeyQueryWriter(ISchema schema)
            : base(schema, new SqlServerDialect()) { }
    }
}