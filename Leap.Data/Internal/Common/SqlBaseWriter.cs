namespace Leap.Data.Internal.Common {
    using System.Text;

    using Leap.Data.Internal.QueryWriter;
    using Leap.Data.Schema;
    using Leap.Data.Utilities;

    abstract class SqlBaseWriter {
        private readonly ISqlDialect sqlDialect;

        private readonly ISchema schema;

        protected SqlBaseWriter(ISqlDialect sqlDialect, ISchema schema) {
            this.sqlDialect = sqlDialect;
            this.schema     = schema;
        }

        protected void WriteColumns<TEntity>(StringBuilder builder) {
            var table = this.schema.GetTable<TEntity>();
            foreach (var columnEntry in table.Columns.AsSmartEnumerable()) {
                builder.Append("t.");
                this.sqlDialect.AppendName(builder, columnEntry.Value.Name);
                if (!columnEntry.IsLast) {
                    builder.Append(",");
                }

                builder.Append(" ");
            }
        }

        protected void WriteWhereClauseForSingleEntity<TEntity, TKey>(TKey key, Command command, StringBuilder builder, bool useAlias = false)
            where TEntity : class {
            var table = this.schema.GetTable<TEntity>();
            foreach (var keyColumnEntry in table.KeyColumnValueExtractor.Extract<TEntity, TKey>(key).AsSmartEnumerable()) {
                if (useAlias) {
                    builder.Append("t.");
                }

                this.sqlDialect.AppendName(builder, keyColumnEntry.Value.Key.Name);
                builder.Append(" = ");
                var paramName = command.AddParameter(keyColumnEntry.Value.Value);
                this.sqlDialect.AddParameter(builder, paramName);
                if (!keyColumnEntry.IsLast) {
                    builder.Append(" and ");
                }
            }
        }

        protected void WriteWhereClauseForMultipleEntities<TEntity, TKey>(TKey[] keys, Command command, StringBuilder builder, bool useAlias = false)
            where TEntity : class {
            var table = this.schema.GetTable<TEntity>();
            foreach (var keyEntry in keys.AsSmartEnumerable()) {
                builder.Append("(");
                foreach (var keyColumnEntry in table.KeyColumnValueExtractor.Extract<TEntity, TKey>(keyEntry.Value).AsSmartEnumerable()) {
                    if (useAlias) {
                        builder.Append("t.");
                    }

                    this.sqlDialect.AppendName(builder, keyColumnEntry.Value.Key.Name);
                    builder.Append(" = ");
                    var paramName = command.AddParameter(keyColumnEntry.Value.Value);
                    this.sqlDialect.AddParameter(builder, paramName);
                    if (!keyColumnEntry.IsLast) {
                        builder.Append(" and ");
                    }
                }

                builder.Append(")");
                if (!keyEntry.IsLast) {
                    builder.Append(" or ");
                }
            }
        }
    }
}