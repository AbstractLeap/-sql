namespace Leap.Data.Internal.UpdateWriter {
    using System.Linq;
    using System.Text;

    using Fasterflect;

    using Leap.Data.Internal.QueryWriter;
    using Leap.Data.Operations;
    using Leap.Data.Schema;
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
            var key = table.KeyExtractor.Extract<TEntity, TKey>(entity);
            foreach (var keyColumnEntry in table.KeyColumnValueExtractor.Extract<TEntity, TKey>(key)) {
                var paramName = command.AddParameter(keyColumnEntry.Value);
                this.sqlDialect.AddParameter(builder, paramName);
                builder.Append(", ");
            }

            var json = this.serializer.Serialize(entity);
            var jsonParamName = command.AddParameter(json);
            this.sqlDialect.AddParameter(builder, jsonParamName);
            builder.Append(", ");

            var typeParamName = command.AddParameter(typeof(TEntity).AssemblyQualifiedName);
            this.sqlDialect.AddParameter(builder, typeParamName);

            builder.Append(")");
            command.AddQuery(builder.ToString());
        }
    }
}