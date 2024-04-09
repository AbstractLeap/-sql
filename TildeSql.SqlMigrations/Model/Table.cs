namespace TildeSql.SqlMigrations.Model {
    using System;
    using System.Collections.Generic;

    public class Table {
        public string Name { get; set; }

        public string Schema { get; set; }

        public string PrimaryKeyName { get; set; }

        public ICollection<Column> Columns { get; set; }

        public ICollection<Index> Indexes { get; set; }

        public bool Equals(Table other, bool ignoreSchemaInTableNameMatching) {
            return this.Name == other.Name && (ignoreSchemaInTableNameMatching || this.Schema == other.Schema);
        }

        public override bool Equals(object obj) {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((Table)obj, ignoreSchemaInTableNameMatching: true);
        }

        public override int GetHashCode() {
            return HashCode.Combine(this.Name);
        }
    }
}