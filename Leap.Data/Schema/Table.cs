namespace Leap.Data.Schema {
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;

    using Leap.Data.Internal;
    using Leap.Data.Internal.ColumnValueFactories;
    using Leap.Data.Schema.Columns;
    using Leap.Data.Utilities;

    /// <summary>
    ///     metadata
    /// </summary>
    public class Table {
        private Column[] allColumns;

        private Column[] nonKeyColumns;
        
        private Column[] keyColumns;

        private Dictionary<string, int> columnIndices;

        private List<Type> entityTypes = new List<Type>();

        public string Name { get; }

        public string Schema { get; }

        public Type KeyType { get; }
        
        public Type BaseEntityType { get; private set; }
        
        public DocumentColumn DocumentColumn { get; }
        
        public DocumentTypeColumn DocumentTypeColumn { get; }
        
        public OptimisticConcurrencyColumn OptimisticConcurrencyColumn { get; private set; }

        public IList<Column> Columns => this.allColumns;

        public IEnumerable<Column> KeyColumns => this.keyColumns;

        public IEnumerable<Column> NonKeyColumns => this.nonKeyColumns;

        public IKeyColumnValueFactory KeyColumnValueExtractor { get; set; }

        public IKeyExtractor KeyExtractor { get; set; }

        public IEnumerable<Type> EntityTypes => this.entityTypes.AsReadOnly();
        
        public bool ContainsTypeHierarchy { get; private set; }

        public int GetColumnIndex(string columnName) {
            if (!this.columnIndices.TryGetValue(columnName, out var index)) {
                throw new Exception($"column with name \"{columnName}\" not found on table {this.Name}");
            }

            return index;
        }

        public Table(string tableName, string schemaName, Type keyType, IEnumerable<(Type Type, string Name)> keyColumns, bool useOptimisticConcurrency = true) {
            this.Name                        = tableName;
            this.Schema                      = schemaName;
            this.KeyType                     = keyType;
            this.keyColumns                  = keyColumns.Select(tuple => new KeyColumn(tuple.Type, tuple.Name, this)).ToArray();
            this.DocumentColumn              = new DocumentColumn(this);
            this.DocumentTypeColumn          = new DocumentTypeColumn(this);
            this.OptimisticConcurrencyColumn = useOptimisticConcurrency ? new OptimisticConcurrencyColumn(this) : null;
            this.nonKeyColumns               = new Column[] { this.DocumentColumn, this.DocumentTypeColumn, this.OptimisticConcurrencyColumn }.Where(c => c != null).ToArray();
            this.allColumns                  = this.keyColumns.Union(this.nonKeyColumns).ToArray();
            this.KeyColumnValueExtractor     = new KeyColumnValueFactory(this);
            this.KeyExtractor                = new DefaultKeyExtractor();
            this.columnIndices               = this.allColumns.Select((c, i) => new { c, i }).ToDictionary(c => c.c.Name, c => c.i);
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

        public void AddClassType(Type entityType) {
            this.entityTypes.Add(entityType);
            if (this.BaseEntityType == null) {
                this.BaseEntityType = entityType;
            }
            else {
                var commonBase = this.entityTypes.FindAssignableWith();
                if (commonBase == null || commonBase == typeof(object)) {
                    throw new Exception("All of the classes inside a single table must have a common base class or interface");
                }

                this.BaseEntityType        = commonBase;
                this.ContainsTypeHierarchy = true;
            }
        }
    }
}