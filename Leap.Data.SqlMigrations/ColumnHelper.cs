namespace Leap.Data.SqlMigrations {
    using System;
    using System.Collections.Generic;

    using Leap.Data.SqlMigrations.Model;

    public static class ColumnHelper {
        private static Dictionary<Type, string>noSizeTypes = new Dictionary<Type, string> {
            { typeof(bool), "AsBoolean" },
            { typeof(byte), "AsByte" },
            { typeof(DateTime), "AsDateTime2" },
            { typeof(DateTimeOffset), "AsDateTimeOffset" },
            { typeof(Double), "AsDouble" },
            { typeof(Guid), "AsGuid" },
            { typeof(float), "AsFloat" },
            { typeof(Int16), "AsInt16" },
            { typeof(Int32), "AsInt32" },
            { typeof(Int64), "AsInt64" },
        };
        
        public static string GenerateColumnType(this Column column) {
            if (noSizeTypes.TryGetValue(column.Type, out var def)) {
                return $".{def}()";
            }

            if (column.Type == typeof(string)) {
                var size = column.Size?.ToString() ?? (column.IsPrimaryKey ? "64" : "Int32.MaxValue");
                return $".AsString({size})";
            }

            // TODO support other types;
            return string.Empty;
        }
    }
}