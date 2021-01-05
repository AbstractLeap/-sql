namespace Leap.Data.SqlServer.QueryWriter {
    using Leap.Data.Internal;
    using Leap.Data.Internal.QueryWriter;
    using Leap.Data.Queries;
    using Leap.Data.Schema;

    public class SqlServerSqlQueryWriter : ISqlQueryWriter {
        private readonly SqlServerSqlKeyQueryWriter sqlKeyQueryWriter;

        private readonly SqlServerSqlMultipleKeyQueryWriter sqlMultipleKeyQueryWriter;

        private readonly SqlServerSqlEntityQueryWriter sqlEntityQueryWriter;

        public SqlServerSqlQueryWriter(ISchema schema) {
            this.sqlKeyQueryWriter         = new SqlServerSqlKeyQueryWriter(schema);
            this.sqlMultipleKeyQueryWriter = new SqlServerSqlMultipleKeyQueryWriter(schema);
            this.sqlEntityQueryWriter      = new SqlServerSqlEntityQueryWriter(schema);
        }

        public void Write(IQuery query, Command command) {
            var genericTypeDefinition = query.GetType().GetGenericTypeDefinition();
            if (genericTypeDefinition == typeof(KeyQuery<,>)) {
                this.sqlKeyQueryWriter.Write(query, command);
            }
            else if (genericTypeDefinition == typeof(MultipleKeyQuery<,>)) {
                this.sqlMultipleKeyQueryWriter.Write(query, command);
            }
            else if (genericTypeDefinition == typeof(EntityQuery<>)) {
                this.sqlEntityQueryWriter.Write(query, command);
            }
        }
    }
}