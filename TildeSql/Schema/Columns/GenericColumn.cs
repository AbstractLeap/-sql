namespace TildeSql.Schema.Columns {
    using System;

    public record GenericColumn : Column {
        public int? Size { get; }

        public int? Precision { get; }

        public bool IsNullable { get; }

        public bool IsIdentity { get; }

        public GenericColumn(Type type, Collection collection, string name, int? size = null, int? precision = null, bool isNullable = false, bool isIdentity = false)
            : base(type, name, collection) {
            this.Size       = size;
            this.Precision  = precision;
            this.IsNullable = isNullable;
            this.IsIdentity = isIdentity;
            this.IsComputed = this.IsIdentity;
        }
    }
}