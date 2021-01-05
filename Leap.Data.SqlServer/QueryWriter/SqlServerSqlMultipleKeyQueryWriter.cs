namespace Leap.Data.SqlServer.QueryWriter {
    using Leap.Data.Internal.QueryWriter;
    using Leap.Data.Schema;

    public class SqlServerSqlMultipleKeyQueryWriter : SqlMultipleKeyQueryWriter {
        public SqlServerSqlMultipleKeyQueryWriter(ISchema schema)
            : base(schema, new SqlServerDialect()) { }
    }
}