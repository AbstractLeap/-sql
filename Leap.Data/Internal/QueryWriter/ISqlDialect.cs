namespace Leap.Data.Internal.QueryWriter {
    using System.Text;

    public interface ISqlDialect {
        void AppendName(StringBuilder builder, string name);

        void AddParameter(StringBuilder builder, string name);

        void AppendPaging(StringBuilder builder, int? queryOffset, int? queryLimit);

        string AddAffectedRowsCount(string sql, Command command);
    }
}