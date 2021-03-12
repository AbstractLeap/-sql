namespace Leap.Data.Schema {
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;

    using Fasterflect;
    
    using Leap.Data.Schema.Columns;
    using Leap.Data.Utilities;

    /// <summary>
    ///     metadata
    /// </summary>
    public class Collection {
        private readonly List<Column> nonKeyColumns;

        private readonly List<KeyColumn> keyColumns;

        private List<Column> allColumns;

        private Dictionary<string, int> columnIndices;

        private readonly List<Type> entityTypes = new List<Type>();
        
        public ICollectionStorageSettings StorageSettings { get; init; }
        
        /// <summary>
        /// The name of the collection (can be different from the collection name)
        /// </summary>
        public string CollectionName { get; }

        public Type KeyType { get; }

        public Type BaseEntityType { get; private set; }

        public DocumentColumn DocumentColumn { get; }

        public DocumentTypeColumn DocumentTypeColumn { get; }

        public OptimisticConcurrencyColumn OptimisticConcurrencyColumn { get; private set; }

        public IReadOnlyList<Column> Columns => this.allColumns;

        public IEnumerable<KeyColumn> KeyColumns => this.keyColumns;

        public IEnumerable<Column> NonKeyColumns => this.nonKeyColumns;

        public IEnumerable<Column> NonComputedColumns => this.allColumns.Where(c => !c.IsComputed);

        public IEnumerable<Column> NonKeyNonComputedColumns => this.nonKeyColumns.Where(c => !c.IsComputed);

        //public IKeyColumnValueFactory KeyColumnValueExtractor { get; set; }

        //public IKeyExtractor KeyExtractor { get; set; }

        public MemberInfo KeyMember { get; set; }

        public IEnumerable<Type> EntityTypes => this.entityTypes.AsReadOnly();

        public bool ContainsTypeHierarchy { get; private set; }

        public int GetColumnIndex(string columnName) {
            if (!this.columnIndices.TryGetValue(columnName, out var index)) {
                throw new Exception($"column with name \"{columnName}\" not found on collection {this.CollectionName}");
            }

            return index;
        }

        public Collection(string collectionName, MemberInfo keyMember, bool useOptimisticConcurrency, bool isKeyComputed) {
            this.IsKeyComputed               = isKeyComputed;
            this.CollectionName              = collectionName;
            this.KeyMember                   = keyMember;
            this.KeyType                     = keyMember.PropertyOrFieldType();
            this.keyColumns                  = this.ResolveKeyColumns(isKeyComputed).ToList();
            this.DocumentColumn              = new DocumentColumn(this);
            this.DocumentTypeColumn          = new DocumentTypeColumn(this);
            this.OptimisticConcurrencyColumn = useOptimisticConcurrency ? new OptimisticConcurrencyColumn(this) : null;
            this.nonKeyColumns               = new Column[] { this.DocumentColumn, this.DocumentTypeColumn, this.OptimisticConcurrencyColumn }.Where(c => c != null).ToList();
            this.RecalculateColumns();
        }

        public bool IsKeyComputed { get; }
        
        public void SetKey<TEntity, TKey>(TEntity entity, TKey key) {
            ReflectionUtils.SetMemberValue(this.KeyMember, entity, key);
        }

        public TKey GetKey<TEntity, TKey>(TEntity entity) {
            return (TKey)ReflectionUtils.GetMemberValue(this.KeyMember, entity);
        }

        public object GetKeyColumnValue<TEntity, TKey>(TKey key, KeyColumn keyColumn) {
            if (keyColumn.MemberInfo == null) { // equivalent to primitive key type (See Below)
                return key;
            }

            return ReflectionUtils.GetMemberValue(keyColumn.MemberInfo, key);
        }

        private IEnumerable<KeyColumn> ResolveKeyColumns(bool isKeyComputed = false) {
            if (this.KeyType.IsPrimitiveKeyType()) {
                return new List<KeyColumn> { new KeyColumn(this.KeyType, this.KeyMember.Name, this, null) { IsComputed = isKeyComputed } };
            }
            
            return new DefaultKeyTypeMemberExtractor().Extract(this.KeyType).Select(m => new KeyColumn(m.PropertyOrFieldType(), m.Name, this, m) { IsComputed = isKeyComputed });
        }

        public void AddClassType(Type entityType) {
            this.entityTypes.Add(entityType);
            if (this.BaseEntityType == null) {
                this.BaseEntityType = entityType;
            }
            else {
                var commonBase = this.entityTypes.FindAssignableWith();
                if (commonBase == null || commonBase == typeof(object)) {
                    throw new Exception("All of the classes inside a single collection must have a common base class or interface");
                }

                this.BaseEntityType        = commonBase;
                this.ContainsTypeHierarchy = true;
            }
        }

        public void AddComputedColumn<T>(string name, string formula) {
            this.nonKeyColumns.Add(new ComputedColumn<T>(this, name, formula));
            this.RecalculateColumns();
        }

        public void AddProjectionColumn<TEntity, TColumn>(string name, Func<TEntity, TColumn> projectionFunc) {
            this.nonKeyColumns.Add(new ProjectionColumn<TEntity, TColumn>(this, name, projectionFunc));
            this.RecalculateColumns();
        }

        private void RecalculateColumns() {
            this.allColumns    = this.keyColumns.Union(this.nonKeyColumns).ToList();
            this.columnIndices = this.allColumns.Select((c, i) => new { c, i }).ToDictionary(c => c.c.Name, c => c.i);
        }

        protected bool Equals(Collection other) {
            return this.CollectionName == other.CollectionName;
        }

        public override bool Equals(object obj) {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return this.Equals((Collection)obj);
        }

        public override int GetHashCode() => HashCode.Combine(this.CollectionName);
    }
}