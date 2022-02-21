namespace TildeSql.Internal.QueryWriter {
    using System.Collections;
    using System.Linq;
    using System.Text;

    using TildeSql.Schema.Conventions.Sql;
    using TildeSql.Utilities;

    using TildeSql.Internal.Common;
    using TildeSql.Queries;
    using TildeSql.Schema;

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
                        var paramName = command.AddParameter(collection.GetTypeName(entry.Value));
                        this.sqlDialect.AddParameter(builder, paramName);
                        if (!entry.IsLast) {
                            builder.Append(" or ");
                        }
                    }

                    builder.Append(")");
                }
            }

            if (!string.IsNullOrWhiteSpace(query.WhereClause)) {
                var whereClause = query.WhereClause;
                if (!whereAppended) {
                    builder.Append(" where ");
                    whereAppended = true;
                }
                else {
                    builder.Append(" and ");
                }

                if (query.WhereClauseParameters != null) {
                    foreach (var parameter in query.WhereClauseParameters) {
                        var enumerable = parameter.Value as IEnumerable;
                        if (enumerable != null && parameter.Value.GetType() != typeof(string)) {
                            // expand the enumerable
                            var values = 0;
                            foreach (var val in enumerable) {
                                values++;
                                command.AddParameter(parameter.Key + "_" + values, val);
                            }

                            var sb = new StringBuilder();
                            this.sqlDialect.AddParameter(sb, parameter.Key);
                            var paramName = sb.ToString();
                            whereClause = whereClause.Replace(paramName, $"({string.Join(",", Enumerable.Range(1, values).Select(i => $"{paramName}_{i}"))})");
                        }
                        else {
                            command.AddParameter(parameter.Key, parameter.Value);
                        }
                    }
                }

                builder.Append("(");
                builder.Append(whereClause);
                builder.Append(")");
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