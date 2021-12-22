namespace TildeSql.Internal.UpdateWriter {
    using System.Text;

    using TildeSql.Schema.Conventions.Sql;
    using TildeSql.Internal.Common;
    using TildeSql.Internal.QueryWriter;
    using TildeSql.Schema;
    using TildeSql.Serialization;

    public abstract class SqlDeleteOperationWriter : SqlBaseWriter {
        private readonly ISchema schema;

        private readonly ISqlDialect sqlDialect;

        private readonly ISerializer serializer;

        protected SqlDeleteOperationWriter(ISchema schema, ISqlDialect sqlDialect, ISerializer serializer)
            : base(sqlDialect, schema) {
            this.schema     = schema;
            this.sqlDialect = sqlDialect;
            this.serializer = serializer;
        }

        public void Write(DatabaseRow databaseRow, Command command) {
            var builder = new StringBuilder("delete from ");
            this.sqlDialect.AppendTableName(builder, databaseRow.Collection.GetTableName(), databaseRow.Collection.GetSchemaName());
            builder.Append(" where ");
            this.WriteWhereClauseForRow(databaseRow, command, builder);
            this.MaybeAddOptimisticConcurrencyWhereClause(builder, command, databaseRow);
            command.AddQuery(builder.ToString());
        }
    }
}