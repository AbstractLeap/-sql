namespace Leap.Data.Internal {
    using Leap.Data.Schema;

    public class DatabaseRow {
        public DatabaseRow(Collection collection, object[] values) {
            this.Collection  = collection;
            this.Values = values;
        }

        public Collection Collection { get; }

        public object[] Values { get; }
    }
}