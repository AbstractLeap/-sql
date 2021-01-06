namespace Leap.Data.Internal {
    using System;

    using Fasterflect;

    using Leap.Data.Schema;

    public static class RowValueHelper {
        public static T GetValue<T>(Table table, object[] row, string columnName) {
            var index = table.GetColumnIndex(columnName);
            return (T)row[index];
        }

        public static object GetValue(Type valueType, Table table, object[] row, string columnName) {
            return typeof(RowValueHelper).CallMethod(new[] { valueType }, nameof(GetValue), new[] { typeof(Table), typeof(object[]), typeof(string) }, table, row, columnName);
        }
    }
}