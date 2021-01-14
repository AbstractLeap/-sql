namespace Leap.Data.Internal {
    using Leap.Data.Schema;

    public class DatabaseRow {
        public DatabaseRow(Table table, object[] values) {
            this.Table  = table;
            this.Values = values;
        }

        public Table Table { get; }

        public object[] Values { get; }
    }
}