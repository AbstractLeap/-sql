﻿namespace Leap.Data.Internal.UpdateWriter {
    using System.Linq;
    using System.Text;

    using Fasterflect;

    using Leap.Data.Internal.Common;
    using Leap.Data.Internal.QueryWriter;
    using Leap.Data.Operations;
    using Leap.Data.Schema;

    internal abstract class SqlUpdateOperationWriter : SqlBaseWriter {
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
            // TODO optimistic concurrency
            // TODO update databaserow 
            var entity = operation.Document.Entity;
            var table = this.schema.GetTable<TEntity>();
            var builder = new StringBuilder("update ");
            this.sqlDialect.AppendName(builder, table.Name);
            builder.Append(" set ");
            this.sqlDialect.AppendName(builder, SpecialColumns.Document);
            builder.Append(" = ");
            var json = this.serializer.Serialize(entity);
            var jsonParamName = command.AddParameter(json);
            this.sqlDialect.AddParameter(builder, jsonParamName);
            builder.Append(" where ");
            this.WriteWhereClauseForSingleEntity<TEntity, TKey>(operation.Key, command, builder);
            command.AddQuery(builder.ToString());
        }
    }
}