namespace Leap.Data.Internal.QueryWriter.SqlServer {
    using Leap.Data.Queries;
    using Leap.Data.Schema;

    internal class SqlServerSqlQueryWriter : ISqlQueryWriter {
        private readonly SqlServerSqlKeyQueryWriter sqlKeyQueryWriter;

        private readonly SqlServerSqlMultipleKeyQueryWriter sqlMultipleKeyQueryWriter;

        public SqlServerSqlQueryWriter(ISchema schema) {
            this.sqlKeyQueryWriter         = new SqlServerSqlKeyQueryWriter(schema);
            this.sqlMultipleKeyQueryWriter = new SqlServerSqlMultipleKeyQueryWriter(schema);
        }

        public void Write(IQuery query, Command command) {
            var genericTypeDefinition = query.GetType().GetGenericTypeDefinition();
            if (genericTypeDefinition == typeof(KeyQuery<,>)) {
                this.sqlKeyQueryWriter.Write(query, command);
            } else if (genericTypeDefinition == typeof(MultipleKeyQuery<,>)) {
                this.sqlMultipleKeyQueryWriter.Write(query, command);
            }
        }
    }
}