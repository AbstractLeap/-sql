﻿namespace Leap.Data.SqlServer {
    using System;
    using System.Text;

    using Leap.Data.Internal;
    using Leap.Data.Internal.QueryWriter;
    using Leap.Data.Schema.Columns;
    using Leap.Data.Schema.Conventions.Sql;
    using Leap.Data.Utilities;

    public class SqlServerDialect : ISqlDialect {
        public void AppendColumnName(StringBuilder builder, string columnName) {
            this.AppendQuotedName(builder, columnName);
        }

        public void AppendTableName(StringBuilder builder, string tableName, string schema) {
            this.AppendQuotedName(builder, schema);
            builder.Append(".");
            this.AppendQuotedName(builder, tableName);
        }

        private void AppendQuotedName(StringBuilder builder, string name) {
            builder.Append("[").Append(name).Append("]");
        }

        public void AddParameter(StringBuilder builder, string name) {
            builder.Append("@").Append(name);
        }

        public void AppendPaging(StringBuilder builder, int? queryOffset, int? queryLimit) {
            builder.Append("offset ").Append(queryOffset ?? 0).Append(" rows ");
            if (queryLimit.HasValue) {
                builder.Append(" fetch next ").Append(queryLimit.Value).Append(" rows only");
            }
        }

        public string AddAffectedRowsCount(string sql, Command command) {
            return sql + "; select @@ROWCOUNT";
        }

        public string PreparePatchIdAndReturn(Column computedKeyColumn) {
            if (!DbTypeSqlMap.TryGetValue(computedKeyColumn.Type, out var dbTypeName)) {
                throw new InvalidOperationException($"Unable to get dbTypeName from {computedKeyColumn.Type}");
            }

            return $"declare @Id table (id {dbTypeName});";
        }

        public string OutputId(Column computedKeyColumn) {
            return $"output inserted.{computedKeyColumn.Name} into @Id";
        }

        public string PatchIdAndReturn(Column computedKeyColumn) {
            var builder = new StringBuilder("update t set ");
            this.AppendColumnName(builder, computedKeyColumn.Collection.DocumentColumn.Name);
            builder.Append(" = JSON_MODIFY(");
            this.AppendColumnName(builder, computedKeyColumn.Collection.DocumentColumn.Name);
            builder.Append(", '$.").Append(computedKeyColumn.Collection.GetKeyPath()).Append("', i.id)");
            builder.Append(" from @Id i join ");
            this.AppendTableName(builder, computedKeyColumn.Collection.GetTableName(), computedKeyColumn.Collection.GetSchemaName());
            builder.Append(" as t on i.id = t.");
            this.AppendColumnName(builder, computedKeyColumn.Name);
            builder.Append("; select top 1 id from @Id");
            return builder.ToString();
        }
    }
}