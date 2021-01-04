namespace Leap.Data.Schema {
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using Leap.Data.Internal;

    /// <summary>
    ///     metadata
    /// </summary>
    public class Table {
        private Column[] allColumns;

        private Column[] nonKeyColumns;
        
        private Column[] keyColumns;

        public string Name { get; }

        public string Schema { get; }

        public Type KeyType { get; }
        
        public DocumentColumn DocumentColumn { get; }
        
        public DocumentTypeColumn DocumentTypeColumn { get; }

        public IList<Column> Columns => this.allColumns;

        public IEnumerable<Column> KeyColumns => this.keyColumns;

        public IEnumerable<Column> NonKeyColumns => this.nonKeyColumns;

        public IKeyColumnValueExtractor KeyColumnValueExtractor { get; set; }

        public IKeyExtractor KeyExtractor { get; set; }

        public Table(string tableName, string schemaName, Type keyType, IEnumerable<Column> keyColumns) {
            this.Name                    = tableName;
            this.Schema                  = schemaName;
            this.KeyType                 = keyType;
            this.keyColumns              = keyColumns.ToArray();
            this.DocumentColumn          = new DocumentColumn();
            this.DocumentTypeColumn      = new DocumentTypeColumn();
            this.KeyColumnValueExtractor = new DefaultKeyColumnValueExtractor(this);
            this.KeyExtractor            = new DefaultKeyExtractor();
            this.nonKeyColumns           = new Column[] { this.DocumentColumn, this.DocumentTypeColumn };
            this.allColumns              = this.keyColumns.Union(this.nonKeyColumns).ToArray();
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