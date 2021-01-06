namespace Leap.Data.Internal.UpdateWriter {
    using System.Linq;
    using System.Text;

    using Fasterflect;

    using Leap.Data.Internal.ColumnValueFactories;
    using Leap.Data.Internal.Common;
    using Leap.Data.Internal.QueryWriter;
    using Leap.Data.Operations;
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

        public void Write(IOperation operation, Command command) {
            var genericTypes = operation.GetType().GetGenericArguments().ToArray();
            this.CallMethod(genericTypes, nameof(Write), operation, command);
        }

        private void Write<TEntity, TKey>(UpdateOperation<TEntity, TKey> operation, Command command)
            where TEntity : class {
            var entity = operation.Document.Entity;
            var table = this.schema.GetTable<TEntity>();
            var builder = new StringBuilder("update ");
            this.sqlDialect.AppendName(builder, table.Name);
            builder.Append(" set ");
            
            var columnValueFactoryFactory = new ColumnValueFactoryFactory(this.serializer);
            foreach (var entry in table.NonKeyColumns.AsSmartEnumerable()) {
                var nonKeyColumn = entry.Value;
                this.sqlDialect.AppendName(builder, nonKeyColumn.Name);
                builder.Append(" = ");
                var columnValue = columnValueFactoryFactory.GetFactory(nonKeyColumn).GetValue<TEntity, TKey>(nonKeyColumn, entity, operation.Document);
                var paramName = command.AddParameter(columnValue);
                this.sqlDialect.AddParameter(builder, paramName);
                if (!entry.IsLast) {
                    builder.Append(", ");
                }
            }

            builder.Append(" where ");
            this.WriteWhereClauseForSingleEntity<TEntity, TKey>(operation.Key, command, builder);
            this.MaybeAddOptimisticConcurrencyWhereClause(builder, command, operation.Document);
            command.AddQuery(builder.ToString());
        }
    }
}