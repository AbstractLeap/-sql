namespace Leap.Data.SqlServer {
    using System.Text;

    using Leap.Data.Internal.QueryWriter;

    public class SqlServerDialect : ISqlDialect {
        public void AppendName(StringBuilder builder, string name) {
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
    }
}