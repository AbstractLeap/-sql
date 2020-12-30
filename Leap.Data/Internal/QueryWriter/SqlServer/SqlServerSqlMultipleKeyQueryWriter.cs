namespace Leap.Data.Internal.QueryWriter.SqlServer {
    using Leap.Data.Internal.Common.SqlServer;
    using Leap.Data.Schema;

    internal class SqlServerSqlMultipleKeyQueryWriter : SqlMultipleKeyQueryWriter {
        public SqlServerSqlMultipleKeyQueryWriter(ISchema schema)
            : base(schema, new SqlServerDialect()) { }
    }
}