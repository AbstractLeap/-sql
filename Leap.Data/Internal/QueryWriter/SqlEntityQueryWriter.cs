namespace Leap.Data.Internal.QueryWriter {
    using System;
    using System.Text;

    using Fasterflect;

    using Leap.Data.Internal.Common;
    using Leap.Data.Queries;
    using Leap.Data.Schema;

    internal abstract class SqlEntityQueryWriter : SqlBaseWriter, ISqlQueryWriter {
        private readonly ISchema schema;

        private readonly ISqlDialect sqlDialect;

        protected SqlEntityQueryWriter(ISchema schema, ISqlDialect sqlDialect)
            : base(sqlDialect, schema) {
            this.schema     = schema;
            this.sqlDialect = sqlDialect;
        }

        public void Write(IQuery query, Command command) {
            if (query.GetType().Name != typeof(EntityQuery<>).Name) {
                throw new Exception($"{query.GetType()} is not EntityQuery<>");
            }

            var genericTypeParameters = query.GetType().GetGenericArguments();
            this.CallMethod(genericTypeParameters, nameof(Write), new[] { query.GetType(), typeof(Command) }, Flags.InstancePrivate | Flags.ExactBinding, query, command);
        }

        private void Write<TEntity>(EntityQuery<TEntity> query, Command command)
            where TEntity : class {
            var table = this.schema.GetTable<TEntity>();

            var builder = new StringBuilder("select ");
            this.WriteColumns<TEntity>(builder);

            builder.Append("from ");
            this.sqlDialect.AppendName(builder, table.Name);
            builder.Append(" as t");

            if (!string.IsNullOrWhiteSpace(query.WhereClause)) {
                if (!query.WhereClause.TrimStart().StartsWith("where ", StringComparison.InvariantCultureIgnoreCase)) {
                    builder.Append(" where ");
                }

                builder.Append(query.WhereClause);
            }

            if (!string.IsNullOrWhiteSpace(query.OrderByClause)) {
                if (!query.OrderByClause.TrimStart().StartsWith("order by ", StringComparison.InvariantCultureIgnoreCase))
                {
                    builder.Append(" order by ");
                }

                builder.Append(query.OrderByClause);
            }

            if (query.Offset.HasValue || query.Limit.HasValue) {
                this.sqlDialect.AppendPaging(builder, query.Offset, query.Limit);
            }
            
            command.AddQuery(builder.ToString());
        }
    }
}