namespace Leap.Data.Internal.QueryWriter.SqlServer {
    using System.Text;

    internal class SqlServerDialect : ISqlDialect {
        public void AppendName(StringBuilder builder, string name) {
            builder.Append("[").Append(name).Append("]");
        }

        public void AddParameter(StringBuilder builder, string name) {
            builder.Append("@").Append(name);
        }
    }
}