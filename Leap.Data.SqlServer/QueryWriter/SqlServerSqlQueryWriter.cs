namespace Leap.Data.SqlServer.QueryWriter {
    using Leap.Data.Internal;
    using Leap.Data.Internal.QueryWriter;
    using Leap.Data.Queries;
    using Leap.Data.Schema;

    public class SqlServerSqlQueryWriter : ISqlQueryWriter, IQueryVisitor {
        private readonly SqlServerSqlKeyQueryWriter sqlKeyQueryWriter;

        private readonly SqlServerSqlMultipleKeyQueryWriter sqlMultipleKeyQueryWriter;

        private readonly SqlServerSqlEntityQueryWriter sqlEntityQueryWriter;

        private Command command;

        public SqlServerSqlQueryWriter(ISchema schema) {
            this.sqlKeyQueryWriter         = new SqlServerSqlKeyQueryWriter(schema);
            this.sqlMultipleKeyQueryWriter = new SqlServerSqlMultipleKeyQueryWriter(schema);
            this.sqlEntityQueryWriter      = new SqlServerSqlEntityQueryWriter(schema);
        }

        public void Write(IQuery query, Command command) {
            this.command = command;
            query.Accept(this);
        }

        public void VisitEntityQuery<TEntity>(EntityQuery<TEntity> entityQuery) where TEntity : class {
            this.sqlEntityQueryWriter.Write(entityQuery, this.command);
        }

        public void VisitKeyQuery<TEntity, TKey>(KeyQuery<TEntity, TKey> keyQuery) where TEntity : class {
            this.sqlKeyQueryWriter.Write(keyQuery, this.command);
        }

        public void VisitMultipleKeyQuery<TEntity, TKey>(MultipleKeyQuery<TEntity, TKey> multipleKeyQuery) where TEntity : class {
            this.sqlMultipleKeyQueryWriter.Write(multipleKeyQuery, this.command);
        }
    }
}