namespace Leap.Data.Schema {
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using Leap.Data.Internal;

    /// <summary>
    ///     metadata
    /// </summary>
    class Table {
        public string Name { get; init; }

        public string Schema { get; init; }

        public Type KeyType { get; init; }

        public IList<Column> Columns { get; init; }

        public IList<Column> KeyColumns { get; init; }

        public IKeyColumnValueExtractor KeyColumnValueExtractor { get; set; }

        public IKeyExtractor KeyExtractor { get; set; }

        public Table(ISchema schema) {
            this.KeyColumnValueExtractor = new DefaultKeyColumnValueExtractor(this);
            this.KeyExtractor            = new DefaultKeyExtractor();
        }

        public IEnumerable<Column> NonKeyColumns() {
            return this.Columns.Except(this.KeyColumns);
        }

        protected bool Equals(Table other) {
            return this.Name == other.Name && this.Schema == other.Schema;
        }

        public override bool Equals(object obj) {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return this.Equals((Table)obj);
        }

        public override int GetHashCode() {
            return HashCode.Combine(this.Name, this.Schema);
        }
    }
}