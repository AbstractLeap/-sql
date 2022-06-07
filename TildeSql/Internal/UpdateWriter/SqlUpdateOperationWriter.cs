namespace TildeSql.Internal.UpdateWriter {
    using System.Text;

    using TildeSql.Internal.Common;
    using TildeSql.Internal.QueryWriter;
    using TildeSql.Schema;
    using TildeSql.Schema.Conventions.Sql;
    using TildeSql.Serialization;
    using TildeSql.Utilities;

    public abstract class SqlUpdateOperationWriter : SqlBaseWriter {
        private readonly ISchema schema;

        private readonly ISqlDialect sqlDialect;

        private readonly ISerializer serializer;

        protected SqlUpdateOperationWriter(ISchema schema, ISqlDialect sqlDialect, ISerializer serializer)
            : base(sqlDialect, schema) {
            this.schema = schema;
            this.sqlDialect = sqlDialect;
            this.serializer = serializer;
        }

        public void Write((DatabaseRow OldDatabaseRow, DatabaseRow NewDatabaseRow) update, Command command) {
            var builder = new StringBuilder("update ");
            var collection = update.OldDatabaseRow.Collection;
            this.sqlDialect.AppendTableName(builder, collection.GetTableName(), collection.GetSchemaName());
            builder.Append(" set ");

            foreach (var entry in collection.NonKeyNonComputedColumns.AsSmartEnumerable()) {
                var nonKeyColumn = entry.Value;
                this.sqlDialect.AppendColumnName(builder, nonKeyColumn.Name);
                builder.Append(" = ");
                var columnValue = update.NewDatabaseRow.Values[update.NewDatabaseRow.Collection.GetColumnIndex(nonKeyColumn.Name)];
                var paramName = command.AddParameter(nonKeyColumn.Name, columnValue);
                this.sqlDialect.AddParameter(builder, paramName);
                if (!entry.IsLast) {
                    builder.Append(", ");
                }
            }

            builder.Append(" where ");
            this.WriteWhereClauseForRow(update.NewDatabaseRow, command, builder);
            this.MaybeAddOptimisticConcurrencyWhereClause(builder, command, update.OldDatabaseRow);
            command.AddQuery(builder.ToString());
        }
    }
}