namespace TildeSql.Schema {
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;

    using Fasterflect;

    using TildeSql.Internal;
    using TildeSql.Schema.Columns;
    using TildeSql.Schema.KeyFactories;
    using TildeSql.Utilities;

    /// <summary>
    ///     metadata
    /// </summary>
    public class Collection {
        private readonly List<Column> nonKeyColumns;

        private readonly List<KeyColumn> keyColumns;

        private readonly IDictionary<KeyColumn, IKeyColumnValueAccessor> keyColumnValueAccessors;

        private List<Column> allColumns;

        private Dictionary<string, int> columnIndices;

        private TypeDeserializer typeDeserializer;

        private readonly List<Type> entityTypes = new();

        public IEnumerable<Type> EntityTypes => this.entityTypes.AsReadOnly();

        public ICollectionStorageSettings StorageSettings { get; init; }
        
        /// <summary>
        /// The name of the collection (can be different from the collection name)
        /// </summary>
        public string CollectionName { get; }

        public Type KeyType { get; }

        public IKeyFactory KeyFactory { get; }

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

        public MemberInfo[] KeyMembers { get; set; }

        public bool ContainsTypeHierarchy { get; private set; }

        public int GetColumnIndex(string columnName) {
            if (columnName == null) throw new ArgumentNullException(nameof(columnName));
            if (!this.columnIndices.TryGetValue(columnName.ToLowerInvariant(), out var index)) {
                throw new Exception($"column with name \"{columnName}\" not found on collection {this.CollectionName}");
            }

            return index;
        }

        public Collection(string collectionName, MemberInfo[] keyMembers, bool useOptimisticConcurrency, bool isKeyComputed) {
            this.IsKeyComputed               = isKeyComputed;
            this.CollectionName              = collectionName;
            this.KeyMembers                  = keyMembers;
            this.KeyType                     = ResolveKeyType(keyMembers);
            var keyColumnResolver = new KeyColumnResolver(this.KeyType, this.KeyMembers, this);
            var keyColumnsAndAccessors = keyColumnResolver.ResolveKeyColumns(isKeyComputed).ToList();
            this.keyColumns                  = keyColumnsAndAccessors.Select(t => t.Item1).ToList();
            this.keyColumnValueAccessors     = keyColumnsAndAccessors.ToDictionary(t => t.Item1, t => t.Item2);
            this.DocumentColumn              = new DocumentColumn(this);
            this.DocumentTypeColumn          = new DocumentTypeColumn(this);
            this.OptimisticConcurrencyColumn = useOptimisticConcurrency ? new OptimisticConcurrencyColumn(this) : null;
            this.nonKeyColumns               = new Column[] { this.DocumentColumn, this.DocumentTypeColumn, this.OptimisticConcurrencyColumn }.Where(c => c != null).ToList();
            this.RecalculateColumns();
            this.KeyFactory = this.KeyMembers.Length > 1
                                  ? new TupleKeyFactory(this.keyColumns.ToArray(), this.KeyType)
                                  : (this.KeyType.IsPrimitiveType() ? new PrimitiveKeyFactory() : new MultipleKeyFactory(this.keyColumns.ToArray(), this.KeyType));
        }

        private static Type ResolveKeyType(MemberInfo[] keyMembers) {
            if (keyMembers.Length == 1) {
                return keyMembers[0].PropertyOrFieldType();
            }

            Type openType;
            switch (keyMembers.Length) {
                case 2:
                    openType = typeof(ValueTuple<,>);
                    break;

                case 3:
                    openType = typeof(ValueTuple<,,>);
                    break;

                case 4:
                    openType = typeof(ValueTuple<,,,>);
                    break;

                case 5:
                    openType = typeof(ValueTuple<,,,,>);
                    break;

                case 6:
                    openType = typeof(ValueTuple<,,,,,>);
                    break;

                default:
                    throw new NotImplementedException("What kind of primary key is this? :-)");
            }

            return openType.MakeGenericType(keyMembers.Select(m => m.PropertyOrFieldType()).ToArray());
        }

        public bool IsKeyComputed { get; }
        
        public void SetKey<TEntity, TKey>(TEntity entity, TKey key) {
            foreach (var memberInfo in this.KeyMembers) {
                ReflectionUtils.SetMemberValue(memberInfo, entity, key);
            }
        }

        public TKey GetKey<TEntity, TKey>(TEntity entity) {
            if (this.KeyMembers.Length == 1) {
                return (TKey)ReflectionUtils.GetMemberValue(this.KeyMembers[0], entity);
            }

            var keyValues = this.KeyMembers.Select(mi => ReflectionUtils.GetMemberValue(mi, entity)).ToArray();
            return (TKey)this.KeyType.CreateInstance(keyValues);
        }

        public object GetKeyColumnValue<TEntity, TKey>(TKey key, KeyColumn keyColumn) {
            return this.keyColumnValueAccessors[keyColumn].GetValue(key);
        }

        public void AddClassType(Type entityType) {
            this.entityTypes.Add(entityType);

            if (this.BaseEntityType == null) {
                this.BaseEntityType = entityType;
            }
            else {
                var commonBase = this.EntityTypes.FindAssignableWith();
                if (commonBase == null || commonBase == typeof(object)) {
                    throw new Exception("All of the classes inside a single collection must have a common base class or interface");
                }

                this.BaseEntityType        = commonBase;
                this.ContainsTypeHierarchy = true;
            }
        }

        public void AddComputedColumn<T>(string name, string formula, bool persisted, bool indexed) {
            this.nonKeyColumns.Add(new ComputedColumn(typeof(T), this, name, formula, persisted, indexed));
            this.RecalculateColumns();
        }

        public void AddProjectionColumn<TEntity, TColumn>(string name, Func<TEntity, TColumn> projectionFunc) {
            this.nonKeyColumns.Add(new ProjectionColumn<TEntity, TColumn>(this, name, projectionFunc));
            this.RecalculateColumns();
        }

        public ITypeDeserializer TypeDeserializer {
            get {
                if (this.typeDeserializer != null) {
                    return this.typeDeserializer;
                }

                this.Finalise();
                return this.typeDeserializer;
            }
        }

        private void RecalculateColumns() {
            this.allColumns    = this.keyColumns.Union(this.nonKeyColumns).ToList();
            this.columnIndices = this.allColumns.Select((c, i) => new { c, i }).ToDictionary(c => c.c.Name.ToLowerInvariant(), c => c.i);
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

        public void Finalise() {
            this.typeDeserializer = new TypeDeserializer(this.entityTypes);
        }
    }
}