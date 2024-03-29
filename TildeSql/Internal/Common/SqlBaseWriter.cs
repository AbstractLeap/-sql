﻿namespace TildeSql.Internal.Common {
    using System.Text;

    using TildeSql.Internal.QueryWriter;
    using TildeSql.Schema;
    using TildeSql.Utilities;

    public abstract class SqlBaseWriter {
        private readonly ISqlDialect sqlDialect;

        private readonly ISchema schema;

        protected SqlBaseWriter(ISqlDialect sqlDialect, ISchema schema) {
            this.sqlDialect = sqlDialect;
            this.schema = schema;
        }

        protected void WriteColumns<TEntity>(StringBuilder builder, Collection collection) {
            foreach (var columnEntry in collection.Columns.AsSmartEnumerable()) {
                builder.Append("t.");
                this.sqlDialect.AppendColumnName(builder, columnEntry.Value.Name);
                if (!columnEntry.IsLast) {
                    builder.Append(",");
                }

                builder.Append(" ");
            }
        }

        protected void WriteWhereClauseForSingleEntity<TEntity, TKey>(TKey key, Command command, Collection collection, StringBuilder builder, bool useAlias = false)
            where TEntity : class {
            foreach (var keyColumnEntry in collection.KeyColumns.AsSmartEnumerable()) {
                var keyColumn = keyColumnEntry.Value;
                if (useAlias) {
                    builder.Append("t.");
                }

                this.sqlDialect.AppendColumnName(builder, keyColumn.Name);
                builder.Append(" = ");
                var paramName = command.AddParameter(keyColumn.Name, collection.GetKeyColumnValue<TEntity, TKey>(key, keyColumn));
                this.sqlDialect.AddParameter(builder, paramName);
                if (!keyColumnEntry.IsLast) {
                    builder.Append(" and ");
                }
            }
        }

        protected void WriteWhereClauseForRow(DatabaseRow databaseRow, Command command, StringBuilder builder, bool useAlias = false) {
            foreach (var keyColumnEntry in databaseRow.Collection.KeyColumns.AsSmartEnumerable()) {
                var keyColumn = keyColumnEntry.Value;
                if (useAlias) {
                    builder.Append("t.");
                }

                this.sqlDialect.AppendColumnName(builder, keyColumn.Name);
                builder.Append(" = ");
                var paramName = command.AddParameter(keyColumn.Name, databaseRow.Values[databaseRow.Collection.GetColumnIndex(keyColumn.Name)]);
                this.sqlDialect.AddParameter(builder, paramName);
                if (!keyColumnEntry.IsLast) {
                    builder.Append(" and ");
                }
            }
        }

        protected void WriteWhereClauseForMultipleEntities<TEntity, TKey>(TKey[] keys, Command command, Collection collection, StringBuilder builder, bool useAlias = false)
            where TEntity : class {
            foreach (var keyEntry in keys.AsSmartEnumerable()) {
                builder.Append("(");
                foreach (var keyColumnEntry in collection.KeyColumns.AsSmartEnumerable()) {
                    var keyColumn = keyColumnEntry.Value;
                    if (useAlias) {
                        builder.Append("t.");
                    }

                    this.sqlDialect.AppendColumnName(builder, keyColumn.Name);
                    builder.Append(" = ");
                    var paramName = command.AddParameter(keyColumn.Name, collection.GetKeyColumnValue<TEntity, TKey>(keyEntry.Value, keyColumn));
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

        protected void MaybeAddOptimisticConcurrencyWhereClause(StringBuilder builder, Command command, DatabaseRow databaseRow, bool appendWhere = false) {
            if (databaseRow.Collection.OptimisticConcurrencyColumn != null) {
                builder.Append(appendWhere ? " where " : " and ");
                this.sqlDialect.AppendColumnName(builder, databaseRow.Collection.OptimisticConcurrencyColumn.Name);
                builder.Append(" = ");
                var paramName = command.AddParameter(databaseRow.Collection.OptimisticConcurrencyColumn.Name,
                    RowValueHelper.GetValue(databaseRow.Collection.OptimisticConcurrencyColumn.Type, databaseRow.Collection, databaseRow.Values, databaseRow.Collection.OptimisticConcurrencyColumn.Name));
                this.sqlDialect.AddParameter(builder, paramName);
            }
        }
    }
}