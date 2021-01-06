﻿namespace Leap.Data.Internal.Common {
    using System.Text;

    using Leap.Data.IdentityMap;
    using Leap.Data.Internal.QueryWriter;
    using Leap.Data.Internal.ColumnValueFactories;
    using Leap.Data.Schema;
    using Leap.Data.Utilities;

    public abstract class SqlBaseWriter {
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
            foreach (var keyColumnEntry in table.KeyColumns.AsSmartEnumerable()) {
                var keyColumn = keyColumnEntry.Value;
                if (useAlias) {
                    builder.Append("t.");
                }

                this.sqlDialect.AppendName(builder, keyColumn.Name);
                builder.Append(" = ");
                var paramName = command.AddParameter(table.KeyColumnValueExtractor.GetValue<TEntity, TKey>(keyColumn, key));
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
                foreach (var keyColumnEntry in table.KeyColumns.AsSmartEnumerable())
                {
                    var keyColumn = keyColumnEntry.Value;
                    if (useAlias) {
                        builder.Append("t.");
                    }

                    this.sqlDialect.AppendName(builder, keyColumn.Name);
                    builder.Append(" = ");
                    var paramName = command.AddParameter(table.KeyColumnValueExtractor.GetValue<TEntity, TKey>(keyColumn, keyEntry.Value));
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

        protected void MaybeAddOptimisticConcurrencyWhereClause<TEntity>(StringBuilder builder, Command command, Document<TEntity> document, bool appendWhere = false) {
            var table = this.schema.GetTable<TEntity>();
            if (table.OptimisticConcurrencyColumn != null) {
                if (appendWhere) {
                    builder.Append(" where ");
                }
                else {
                    builder.Append(" and ");
                }

                this.sqlDialect.AppendName(builder, table.OptimisticConcurrencyColumn.Name);
                builder.Append(" = ");
                var paramName = command.AddParameter(
                    RowValueHelper.GetValue(table.OptimisticConcurrencyColumn.Type, table, document.Row.Values, table.OptimisticConcurrencyColumn.Name));
                this.sqlDialect.AddParameter(builder, paramName);
            }
        }
    }
}