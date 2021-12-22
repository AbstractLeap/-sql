namespace TildeSql.Internal {
    using System;

    using Fasterflect;

    using TildeSql.Schema;

    public static class RowValueHelper {
        public static T GetValue<T>(Collection collection, object[] row, string columnName) {
            var index = collection.GetColumnIndex(columnName);
            return (T)row[index];
        }

        public static object GetValue(Type valueType, Collection collection, object[] row, string columnName) {
            return typeof(RowValueHelper).CallMethod(new[] { valueType }, nameof(GetValue), new[] { typeof(Collection), typeof(object[]), typeof(string) }, collection, row, columnName);
        }
    }
}