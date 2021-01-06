namespace Leap.Data.Internal {
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
    }
}