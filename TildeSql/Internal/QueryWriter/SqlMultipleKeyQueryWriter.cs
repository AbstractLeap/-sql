namespace TildeSql.Internal.QueryWriter {
    using System.Text;

    using TildeSql.Schema.Conventions.Sql;

    using TildeSql.Internal.Common;
    using TildeSql.Queries;
    using TildeSql.Schema;

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
            var collection = query.Collection;

            var builder = new StringBuilder("select ");
            this.WriteColumns<TEntity>(builder, collection);

            builder.Append("from ");
            this.sqlDialect.AppendTableName(builder, collection.GetTableName(), collection.GetSchemaName());
            builder.Append(" as t");
            builder.Append(" where ");
            this.WriteWhereClauseForMultipleEntities<TEntity, TKey>(query.Keys, command, collection, builder, true);

            command.AddQuery(builder.ToString());
        }
    }
}