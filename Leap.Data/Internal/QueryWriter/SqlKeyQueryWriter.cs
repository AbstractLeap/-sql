namespace Leap.Data.Internal.QueryWriter {
    using System.Text;

    using Leap.Data.Internal.Common;
    using Leap.Data.Queries;
    using Leap.Data.Schema;

    public abstract class SqlKeyQueryWriter : SqlBaseWriter, ISqlKeyQueryWriter {
        private readonly ISchema schema;

        private readonly ISqlDialect sqlDialect;

        protected SqlKeyQueryWriter(ISchema schema, ISqlDialect sqlDialect)
            : base(sqlDialect, schema) {
            this.schema     = schema;
            this.sqlDialect = sqlDialect;
        }

        public void Write<TEntity, TKey>(KeyQuery<TEntity, TKey> query, Command command)
            where TEntity : class {
            var table = this.schema.GetTable<TEntity>();

            var builder = new StringBuilder("select ");
            this.WriteColumns<TEntity>(builder);

            builder.Append("from ");
            this.sqlDialect.AppendName(builder, table.Name);
            builder.Append(" as t");
            builder.Append(" where ");
            this.WriteWhereClauseForSingleEntity<TEntity, TKey>(query.Key, command, builder, true);

            command.AddQuery(builder.ToString());
        }
    }
}