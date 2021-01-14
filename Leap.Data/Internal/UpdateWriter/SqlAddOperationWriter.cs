namespace Leap.Data.Internal.UpdateWriter {
    using System.Text;

    using Leap.Data.Internal.QueryWriter;
    using Leap.Data.Schema;
    using Leap.Data.Schema.Columns;
    using Leap.Data.Serialization;
    using Leap.Data.Utilities;

    public abstract class SqlAddOperationWriter {
        private readonly ISchema schema;

        private readonly ISqlDialect sqlDialect;

        private readonly ISerializer serializer;

        protected SqlAddOperationWriter(ISchema schema, ISqlDialect sqlDialect, ISerializer serializer) {
            this.schema     = schema;
            this.sqlDialect = sqlDialect;
            this.serializer = serializer;
        }

        public void Write(DatabaseRow databaseRow, Command command) {
            var builder = new StringBuilder("insert into ");
            this.sqlDialect.AppendName(builder, databaseRow.Table.Name);
            builder.Append(" (");
            foreach (var entry in databaseRow.Table.Columns.AsSmartEnumerable()) {
                this.sqlDialect.AppendName(builder, entry.Value.Name);
                if (!entry.IsLast) {
                    builder.Append(", ");
                }
            }

            builder.Append(") values (");
            foreach (var entry in databaseRow.Table.Columns.AsSmartEnumerable()) {
                AppendValue(entry.Value, databaseRow.Values);
                if (!entry.IsLast) {
                    builder.Append(", ");
                }
            }

            builder.Append(")");
            command.AddQuery(builder.ToString());

            void AppendValue(Column column, object[] databaseRowValues) {
                var paramName = command.AddParameter(databaseRowValues[databaseRow.Table.GetColumnIndex(column.Name)]);
                this.sqlDialect.AddParameter(builder, paramName);
            }
        }
    }
}