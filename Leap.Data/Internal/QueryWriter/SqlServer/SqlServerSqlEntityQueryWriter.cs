namespace Leap.Data.Internal.QueryWriter.SqlServer {
    using Leap.Data.Internal.Common.SqlServer;
    using Leap.Data.Schema;

    class SqlServerSqlEntityQueryWriter : SqlEntityQueryWriter {
        public SqlServerSqlEntityQueryWriter(ISchema schema)
            : base(schema, new SqlServerDialect()) { }
    }
}