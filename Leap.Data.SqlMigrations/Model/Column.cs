namespace Leap.Data.SqlMigrations.Model {
    using System;

    public class Column {
        public string Name { get; set; }

        public Type Type { get; set; }

        public int? Size { get; set; }

        public int? Precision { get; set; }

        public object DefaultValue { get; set; }

        public bool IsPrimaryKey { get; set; }

        public bool IsNullable { get; set; }

        public bool IsIdentity { get; set; }

        protected bool Equals(Column other) {
            return this.Name == other.Name;
        }

        public override bool Equals(object obj) {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((Column)obj);
        }

        public override int GetHashCode() {
            return (this.Name != null ? this.Name.GetHashCode() : 0);
        }
    }
}