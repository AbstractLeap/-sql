namespace Leap.Data.Internal.QueryWriter {
    using System.Text;

    internal interface ISqlDialect {
        void AppendName(StringBuilder builder, string name);

        void AddParameter(StringBuilder builder, string name);
    }
}