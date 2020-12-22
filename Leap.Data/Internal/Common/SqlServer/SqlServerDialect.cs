namespace Leap.Data.Internal.Common.SqlServer {
    using System.Text;

    using Leap.Data.Internal.QueryWriter;

    internal class SqlServerDialect : ISqlDialect {
        public void AppendName(StringBuilder builder, string name) {
            builder.Append("[").Append(name).Append("]");
        }

        public void AddParameter(StringBuilder builder, string name) {
            builder.Append("@").Append(name);
        }
    }
}