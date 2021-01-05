namespace Leap.Data.Internal.UpdateWriter {
    using System;
    using System.Linq;
    using System.Text;

    using Fasterflect;

    using Leap.Data.Internal.ColumnValueFactories;
    using Leap.Data.Internal.QueryWriter;
    using Leap.Data.Operations;
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

        public void Write(IOperation operation, Command command) {
            var entityType = operation.GetType().GetGenericArguments().First();
            var table = this.schema.GetTable(entityType);
            this.CallMethod(new[] { entityType, table.KeyType }, nameof(Write), operation, command);
        }

        private void Write<TEntity, TKey>(AddOperation<TEntity> operation, Command command) {
            // TODO optimistic concurrency
            // TODO generate databaserow and store in identity map document
            var entity = operation.Entity;
            var table = this.schema.GetTable<TEntity>();
            var builder = new StringBuilder("insert into ");
            this.sqlDialect.AppendName(builder, table.Name);
            builder.Append(" (");
            foreach (var keyColumnEntry in table.KeyColumns.AsSmartEnumerable()) {
                this.sqlDialect.AppendName(builder, keyColumnEntry.Value.Name);
                builder.Append(", ");
            }

            foreach (var columnEntry in table.NonKeyColumns.AsSmartEnumerable()) {
                this.sqlDialect.AppendName(builder, columnEntry.Value.Name);
                if (!columnEntry.IsLast) {
                    builder.Append(", ");
                }
            }

            builder.Append(") values (");
            var columnValueFactoryFactory = new ColumnValueFactoryFactory(this.serializer);
            foreach (var keyColumn in table.KeyColumns) {
                AppendValue(keyColumn);
                builder.Append(", ");
            }

            foreach (var entry in table.NonKeyColumns.AsSmartEnumerable()) {
                var column = entry.Value;
                AppendValue(column);
                if (!entry.IsLast) {
                    builder.Append(", ");
                }
            }

            builder.Append(")");
            command.AddQuery(builder.ToString());

            void AppendValue(Column column) {
                var columnValue = columnValueFactoryFactory.GetFactory(column).GetValue<TEntity, TKey>(column, entity, null); ;
                var paramName = command.AddParameter(columnValue);
                this.sqlDialect.AddParameter(builder, paramName);
            }
        }
    }
}