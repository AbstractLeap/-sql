namespace Leap.Data.Internal.QueryWriter.SqlServer {
    using System.Text;

    using Leap.Data.Schema;

    internal class SqlServerSqlKeyQueryWriter : SqlKeyQueryWriter {
        private readonly SqlServerDialect dialect;

        public SqlServerSqlKeyQueryWriter(ISchema schema)
            : base(schema) {
            this.dialect = new SqlServerDialect();
        }

        protected override void AppendName(StringBuilder builder, string name) {
            this.dialect.AppendName(builder, name);
        }

        protected override void AddParameter(StringBuilder builder, string name) {
            this.dialect.AddParameter(builder, name);
        }
    }
}