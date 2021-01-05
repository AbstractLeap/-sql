namespace Leap.Data.Internal {
    using System;

    using Leap.Data.Schema;

    /// <summary>
    ///     store database rows in the second level cache
    /// </summary>
    public class DatabaseRow {
        public DatabaseRow(Table table, object[] values) {
            this.Table  = table;
            this.Values = values;
        }

        public Table Table { get; }

        public object[] Values { get; }

        public T GetValue<T>(string name) {
            for (var i = 0; i < this.Table.Columns.Count; i++) {
                if (this.Table.Columns[i].Name == name) {
                    return (T)this.Values[i];
                }
            }

            throw new Exception($"Column {name} not found in table {this.Table.Name}");
        }
    }
}