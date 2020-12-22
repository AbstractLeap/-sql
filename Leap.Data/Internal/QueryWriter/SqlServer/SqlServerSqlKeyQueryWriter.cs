namespace Leap.Data.Internal.QueryWriter.SqlServer {
    using Leap.Data.Internal.Common.SqlServer;
    using Leap.Data.Schema;

    internal class SqlServerSqlKeyQueryWriter : SqlKeyQueryWriter {
        public SqlServerSqlKeyQueryWriter(ISchema schema)
            : base(schema, new SqlServerDialect()) { }
    }
}