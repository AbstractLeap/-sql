namespace Leap.Data.Internal.QueryWriter {
    using System.Text;

    using Leap.Data.Schema.Columns;

    public interface ISqlDialect {
        void AppendColumnName(StringBuilder builder, string columnName);

        void AppendTableName(StringBuilder builder, string tableName, string schema);

        void AddParameter(StringBuilder builder, string name);

        void AppendPaging(StringBuilder builder, int? queryOffset, int? queryLimit);

        string AddAffectedRowsCount(string sql, Command command);

        string PatchIdAndReturn(Column computedKeyColumn, uint idCounter);

        string PreparePatchIdAndReturn(Column computedKeyColumn, uint idCounter);

        string OutputId(Column computedKeyColumn, uint idCounter);
    }
}