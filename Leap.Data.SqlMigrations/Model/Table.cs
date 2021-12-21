namespace Leap.Data.SqlMigrations.Model {
    using System;
    using System.Collections.Generic;

    public class Table {
        public string Name { get; set; }

        public string Schema { get; set; }

        public string PrimaryKeyName { get; set; }

        public ICollection<Column> Columns { get; set; }

        public ICollection<Index> Indexes { get; set; }

        protected bool Equals(Table other) {
            return this.Name == other.Name && this.Schema == other.Schema;
        }

        public override bool Equals(object obj) {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((Table)obj);
        }

        public override int GetHashCode() {
            return HashCode.Combine(this.Name, this.Schema);
        }
    }
}