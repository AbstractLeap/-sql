namespace Leap.Data.Internal {
    using Leap.Data.Schema;

    public static class RowValueHelper {
        public static T GetValue<T>(Table table, object[] row, string columnName) {
            var index = table.GetColumnIndex(columnName);
            return (T)row[index];
        }
    }
}