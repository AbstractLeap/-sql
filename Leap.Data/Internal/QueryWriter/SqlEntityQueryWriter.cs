namespace Leap.Data.Internal.QueryWriter {
    using System.Text;

    using Leap.Data.Internal.Common;
    using Leap.Data.Queries;
    using Leap.Data.Schema;
    using Leap.Data.Schema.Conventions.Sql;
    using Leap.Data.Utilities;

    public abstract class SqlEntityQueryWriter : SqlBaseWriter, ISqlEntityQueryWriter {
        private readonly ISchema schema;

        private readonly ISqlDialect sqlDialect;

        protected SqlEntityQueryWriter(ISchema schema, ISqlDialect sqlDialect)
            : base(sqlDialect, schema) {
            this.schema     = schema;
            this.sqlDialect = sqlDialect;
        }

        public void Write<TEntity>(EntityQuery<TEntity> query, Command command)
            where TEntity : class {
            var collection = query.Collection;
            var builder = new StringBuilder("select ");
            this.WriteColumns<TEntity>(builder, collection);

            builder.Append("from ");
            this.sqlDialect.AppendTableName(builder, collection.GetTableName(), collection.GetSchemaName());
            builder.Append(" as t");

            var whereAppended = false;
            if (collection.ContainsTypeHierarchy) {
                if (typeof(TEntity) != collection.BaseEntityType) {
                    // we're querying for some of the derived types only
                    var assignableTypes = typeof(TEntity).GetAssignableTypes(collection.EntityTypes);
                    if (!whereAppended) {
                        builder.Append(" where ");
                        whereAppended = true;
                    }

                    builder.Append("(");
                    foreach (var entry in assignableTypes.AsSmartEnumerable()) {
                        this.sqlDialect.AppendColumnName(builder, collection.DocumentTypeColumn.Name);
                        builder.Append(" = ");
                        var paramName = command.AddParameter(entry.Value.AssemblyQualifiedName);
                        this.sqlDialect.AddParameter(builder, paramName);
                        if (!entry.IsLast) {
                            builder.Append(" or ");
                        }
                    }

                    builder.Append(")");
                }
            }

            if (!string.IsNullOrWhiteSpace(query.WhereClause)) {
                if (!whereAppended) {
                    builder.Append(" where ");
                    whereAppended = true;
                }
                else {
                    builder.Append(" and ");
                }

                builder.Append("(");
                builder.Append(query.WhereClause);
                builder.Append(")");

                if (query.WhereClauseParameters != null) {
                    foreach (var parameter in query.WhereClauseParameters) {
                        command.AddParameter(parameter.Key, parameter.Value);
                    }
                }
            }

            if (!string.IsNullOrWhiteSpace(query.OrderByClause)) {
                builder.Append(" order by ");
                builder.Append(query.OrderByClause);
            }

            if (query.Offset.HasValue || query.Limit.HasValue) {
                this.sqlDialect.AppendPaging(builder, query.Offset, query.Limit);
            }

            command.AddQuery(builder.ToString());
        }
    }
}