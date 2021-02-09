namespace Leap.Data.Internal.QueryWriter {
    using System.Text;

    using Leap.Data.Internal.Common;
    using Leap.Data.Queries;
    using Leap.Data.Schema;
    using Leap.Data.Schema.Conventions.Sql;

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
            var table = query.Table;

            var builder = new StringBuilder("select ");
            this.WriteColumns<TEntity>(builder, table);

            builder.Append("from ");
            this.sqlDialect.AppendTableName(builder, table.GetTableName(), table.GetSchemaName());
            builder.Append(" as t");
            builder.Append(" where ");
            this.WriteWhereClauseForMultipleEntities<TEntity, TKey>(query.Keys, command, table, builder, true);

            command.AddQuery(builder.ToString());
        }
    }
}