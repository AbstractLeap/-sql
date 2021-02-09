namespace Leap.Data.SqlServer {
    using System.Text;

    using Leap.Data.Internal;
    using Leap.Data.Internal.QueryWriter;

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
    }
}