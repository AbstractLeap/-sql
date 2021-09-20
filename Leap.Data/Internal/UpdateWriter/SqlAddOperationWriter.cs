namespace Leap.Data.Internal.UpdateWriter {
    using System;
    using System.Linq;
    using System.Text;
    using System.Threading;

    using Leap.Data.Internal.QueryWriter;
    using Leap.Data.Schema;
    using Leap.Data.Schema.Columns;
    using Leap.Data.Schema.Conventions.Sql;
    using Leap.Data.Serialization;
    using Leap.Data.Utilities;

    public abstract class SqlAddOperationWriter {
        private readonly ISchema schema;

        private readonly ISqlDialect sqlDialect;

        private readonly ISerializer serializer;

        private uint idCounter;

        protected SqlAddOperationWriter(ISchema schema, ISqlDialect sqlDialect, ISerializer serializer) {
            this.schema     = schema;
            this.sqlDialect = sqlDialect;
            this.serializer = serializer;
            this.idCounter  = 1;
        }

        public void Write(DatabaseRow databaseRow, Command command) {
            var counter = Interlocked.Increment(ref this.idCounter);

            var computedKeyColumns = databaseRow.Collection.KeyColumns.Where(c => c.IsComputed).ToArray();
            // TODO support multiple computed key columns
            if (computedKeyColumns.Length > 1) {
                throw new NotImplementedException("We don't support multiple computed key columns yet");
            }

            var builder = new StringBuilder(string.Empty);
            if (computedKeyColumns.Length == 1) {
                builder.Append(this.sqlDialect.PreparePatchIdAndReturn(computedKeyColumns[0], counter));
            }

            builder.Append("insert into ");
            this.sqlDialect.AppendTableName(builder, databaseRow.Collection.GetTableName(), databaseRow.Collection.GetSchemaName());
            builder.Append(" (");
            foreach (var entry in databaseRow.Collection.NonComputedColumns.AsSmartEnumerable()) {
                this.sqlDialect.AppendColumnName(builder, entry.Value.Name);
                if (!entry.IsLast) {
                    builder.Append(", ");
                }
            }

            builder.Append(") ");

            if (computedKeyColumns.Length == 1) {
                builder.Append(this.sqlDialect.OutputId(computedKeyColumns[0], counter));
            }

            builder.Append(" values (");
            foreach (var entry in databaseRow.Collection.NonComputedColumns.AsSmartEnumerable()) {
                AppendValue(entry.Value, databaseRow.Values);
                if (!entry.IsLast) {
                    builder.Append(", ");
                }
            }

            builder.Append(")");

            if (computedKeyColumns.Length == 1) {
                builder.Append(";").Append(this.sqlDialect.PatchIdAndReturn(computedKeyColumns[0], counter++));
            }

            command.AddQuery(builder.ToString());

            void AppendValue(Column column, object[] databaseRowValues) {
                var paramName = command.AddParameter(databaseRowValues[databaseRow.Collection.GetColumnIndex(column.Name)]);
                this.sqlDialect.AddParameter(builder, paramName);
            }
        }
    }
}