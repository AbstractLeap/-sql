namespace Leap.Data.Internal.UpdateWriter {
    using System;
    using System.Text;

    using Leap.Data.Internal.Common;
    using Leap.Data.Internal.QueryWriter;
    using Leap.Data.Schema;
    using Leap.Data.Serialization;
    using Leap.Data.Utilities;

    public abstract class SqlUpdateOperationWriter : SqlBaseWriter {
        private readonly ISchema schema;

        private readonly ISqlDialect sqlDialect;

        private readonly ISerializer serializer;

        protected SqlUpdateOperationWriter(ISchema schema, ISqlDialect sqlDialect, ISerializer serializer)
            : base(sqlDialect, schema) {
            this.schema     = schema;
            this.sqlDialect = sqlDialect;
            this.serializer = serializer;
        }

        public void Write(DatabaseRow databaseRow, Command command) {
            var builder = new StringBuilder("update ");
            this.sqlDialect.AppendName(builder, databaseRow.Table.Name);
            builder.Append(" set ");

            foreach (var entry in databaseRow.Table.NonKeyColumns.AsSmartEnumerable()) {
                var nonKeyColumn = entry.Value;
                this.sqlDialect.AppendName(builder, nonKeyColumn.Name);
                builder.Append(" = ");
                var columnValue = entry.Value == databaseRow.Table.OptimisticConcurrencyColumn
                                      ? Guid.NewGuid()
                                      : databaseRow.Values[databaseRow.Table.GetColumnIndex(nonKeyColumn.Name)];
                var paramName = command.AddParameter(columnValue);
                this.sqlDialect.AddParameter(builder, paramName);
                if (!entry.IsLast) {
                    builder.Append(", ");
                }
            }

            builder.Append(" where ");
            this.WriteWhereClauseForRow(databaseRow, command, builder);
            this.MaybeAddOptimisticConcurrencyWhereClause(builder, command, databaseRow);
            command.AddQuery(builder.ToString());
        }
    }
}