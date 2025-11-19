namespace TildeSql.Internal.QueryWriter {
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Text.RegularExpressions;

    using TildeSql.Internal.Common;
    using TildeSql.Queries;
    using TildeSql.Schema;
    using TildeSql.Schema.Conventions.Sql;
    using TildeSql.Utilities;

    public abstract class SqlEntityQueryWriter : SqlBaseWriter, ISqlEntityQueryWriter {
        private readonly ISchema schema;

        private readonly ISqlDialect sqlDialect;

        protected SqlEntityQueryWriter(ISchema schema, ISqlDialect sqlDialect)
            : base(sqlDialect, schema) {
            this.schema = schema;
            this.sqlDialect = sqlDialect;
        }

        public void Write<TEntity>(EntityQuery<TEntity> query, Command command)
            where TEntity : class {
            var collection = query.Collection;
            var builder = new StringBuilder();
            if (query.CountAccessor != null) {
                builder.Append(";with t as (select ");
            }
            else {
                builder.Append("select ");
            }

            this.WriteColumns<TEntity>(builder, collection);

            builder.Append("from ");
            this.sqlDialect.AppendTableName(builder, collection.GetTableName(), collection.GetSchemaName());
            builder.Append(" as t");

            if (query.WithClauses?.Any() is true) {
                foreach (var withClause in query.WithClauses) {
                    builder.Append(" ");
                    builder.Append(withClause);
                    builder.AppendLine(" ");
                }
            }

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
                        var paramName = command.AddParameter(collection.DocumentTypeColumn.Name, collection.TypeSerializer.Serialize(entry.Value));
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
                            var enumerableParamNames = new List<string>();
                            foreach (var val in enumerable) {
                                enumerableParamNames.Add(command.AddParameter(parameter.Key, val));
                            }

                            var paramName = GetParamSqlName(parameter.Key);
                            var newParams = $"({string.Join(",", enumerableParamNames.Select(GetParamSqlName))})";
                            whereClause = ReplaceParameter(whereClause, paramName, newParams);
                        }
                        else {
                            var actualParamName = command.AddParameter(parameter.Key, parameter.Value);
                            if (actualParamName != parameter.Key) {
                                whereClause = ReplaceParameter(whereClause, GetParamSqlName(parameter.Key), GetParamSqlName(actualParamName));
                            }
                        }
                    }
                }

                builder.Append("(");
                builder.Append(whereClause);
                builder.Append(")");
            }

            if (query.CountAccessor != null) {
                builder.Append(
                    """
                    )
                    , cteCount as (
                     select count_big(1) as TotalCount
                     from t
                    )
                    select * 
                    from t, cteCount
                    """);
            }

            if (!string.IsNullOrWhiteSpace(query.OrderByClause)) {
                builder.Append(" order by ");
                builder.Append(query.OrderByClause);
            }

            if (query.Offset.HasValue || query.Limit.HasValue) {
                this.sqlDialect.AppendPaging(builder, query.Offset, query.Limit);
            }

            command.AddQuery(builder.ToString());

            string GetParamSqlName(string name) {
                var sb = new StringBuilder();
                this.sqlDialect.AddParameter(sb, name); // @{parameter.Key} in Sql Server, for example
                return sb.ToString();
            }

            static string ReplaceParameter(string whereClause, string oldParamName, string newParamName) {
                var regex = new Regex($"({oldParamName})([^\\w]|$)");
                whereClause = regex.Replace(whereClause, m => $"{newParamName}{m.Groups[2].Value}"); // first group is the whole match, second is the existing param, third (zero based remember) is the bit after the variable that we need to put back in
                return whereClause;
            }
        }
    }
}