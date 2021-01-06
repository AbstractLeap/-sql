namespace Leap.Data.Internal.UpdateWriter {
    using System.Linq;
    using System.Text;

    using Fasterflect;

    using Leap.Data.Internal.Common;
    using Leap.Data.Internal.QueryWriter;
    using Leap.Data.Operations;
    using Leap.Data.Schema;
    using Leap.Data.Serialization;

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

        public void Write(IOperation operation, Command command) {
            var entityType = operation.GetType().GetGenericArguments().First();
            var table = this.schema.GetTable(entityType);
            this.CallMethod(new[] { entityType, table.KeyType }, nameof(Write), operation, command);
        }

        private void Write<TEntity, TKey>(DeleteOperation<TEntity> operation, Command command)
            where TEntity : class {
            // TODO optimistic concurrency
            // TODO update databaserow 
            var entity = operation.Document.Entity;
            var table = this.schema.GetTable<TEntity>();
            var builder = new StringBuilder("delete from ");
            this.sqlDialect.AppendName(builder, table.Name);
            builder.Append(" where ");
            this.WriteWhereClauseForSingleEntity<TEntity, TKey>(table.KeyExtractor.Extract<TEntity, TKey>(entity), command, builder);
            this.MaybeAddOptimisticConcurrencyWhereClause(builder, command, operation.Document);
            command.AddQuery(builder.ToString());
        }
    }
}