namespace Leap.Data.Internal.QueryWriter.SqlServer {
    using Leap.Data.Queries;
    using Leap.Data.Schema;

    internal class SqlServerSqlQueryWriter : ISqlQueryWriter {
        private readonly SqlServerSqlKeyQueryWriter sqlKeyQueryWriter;

        public SqlServerSqlQueryWriter(ISchema schema) {
            this.sqlKeyQueryWriter = new SqlServerSqlKeyQueryWriter(schema);
        }

        public void Write(IQuery query, Command command) {
            var genericTypeDefinition = query.GetType().GetGenericTypeDefinition();
            if (genericTypeDefinition == typeof(KeyQuery<,>)) {
                this.sqlKeyQueryWriter.Write(query, command);
            }
        }
    }
}