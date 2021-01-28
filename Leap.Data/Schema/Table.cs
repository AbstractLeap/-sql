namespace Leap.Data.Schema {
    using System;
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
        private readonly List<Column> nonKeyColumns;

        private readonly List<Column> keyColumns;

        private List<Column> allColumns;

        private Dictionary<string, int> columnIndices;

        private readonly List<Type> entityTypes = new List<Type>();

        public string Name { get; }

        public string Schema { get; }

        public Type KeyType { get; }

        public Type BaseEntityType { get; private set; }

        public DocumentColumn DocumentColumn { get; }

        public DocumentTypeColumn DocumentTypeColumn { get; }

        public OptimisticConcurrencyColumn OptimisticConcurrencyColumn { get; private set; }

        public IReadOnlyList<Column> Columns => this.allColumns;

        public IEnumerable<Column> KeyColumns => this.keyColumns;

        public IEnumerable<Column> NonKeyColumns => this.nonKeyColumns;

        public IEnumerable<Column> NonComputedColumns => this.allColumns.Where(c => !c.IsComputed);

        public IEnumerable<Column> NonKeyNonComputedColumns => this.nonKeyColumns.Where(c => !c.IsComputed);

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
            this.keyColumns                  = keyColumns.Select(tuple => new KeyColumn(tuple.Type, tuple.Name, this)).Cast<Column>().ToList();
            this.DocumentColumn              = new DocumentColumn(this);
            this.DocumentTypeColumn          = new DocumentTypeColumn(this);
            this.OptimisticConcurrencyColumn = useOptimisticConcurrency ? new OptimisticConcurrencyColumn(this) : null;
            this.nonKeyColumns               = new Column[] { this.DocumentColumn, this.DocumentTypeColumn, this.OptimisticConcurrencyColumn }.Where(c => c != null).ToList();
            this.KeyColumnValueExtractor     = new KeyColumnValueFactory(this);
            this.KeyExtractor                = new DefaultKeyExtractor();
            this.RecalculateColumns();
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

        public void AddComputedColumn<T>(string name, string formula) {
            this.nonKeyColumns.Add(new ComputedColumn<T>(this, name, formula));
            this.RecalculateColumns();
        }

        private void RecalculateColumns() {
            this.allColumns    = this.keyColumns.Union(this.nonKeyColumns).ToList();
            this.columnIndices = this.allColumns.Select((c, i) => new { c, i }).ToDictionary(c => c.c.Name, c => c.i);
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