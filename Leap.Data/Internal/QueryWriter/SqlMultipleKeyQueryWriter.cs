namespace Leap.Data.Internal.QueryWriter {
    using System.Text;

    using Leap.Data.Internal.Common;
    using Leap.Data.Queries;
    using Leap.Data.Schema;

    public abstract class SqlMultipleKeyQueryWriter : SqlBaseWriter, ISqlMultipleKeyQueryWriter {
        private readonly ISchema schema;

        private readonly ISqlDialect sqlDialect;

        protected SqlMultipleKeyQueryWriter(ISchema schema, ISqlDialect sqlDialect)
            : base(sqlDialect, schema) {
            this.schema     = schema;
            this.sqlDialect = sqlDialect;
        }

        public void Write<TEntity, TKey>(MultipleKeyQuery<TEntity, TKey> query, Command command)
            where TEntity : class {
            var table = this.schema.GetTable<TEntity>();

            var builder = new StringBuilder("select ");
            this.WriteColumns<TEntity>(builder);

            builder.Append("from ");
            this.sqlDialect.AppendName(builder, table.Name);
            builder.Append(" as t");
            builder.Append(" where ");
            this.WriteWhereClauseForMultipleEntities<TEntity, TKey>(query.Keys, command, builder, true);

            command.AddQuery(builder.ToString());
        }
    }
}