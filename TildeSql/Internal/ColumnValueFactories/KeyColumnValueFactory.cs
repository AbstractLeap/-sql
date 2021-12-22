namespace TildeSql.Internal.ColumnValueFactories {
    using Fasterflect;

    using TildeSql.Utilities;

    using TildeSql.Schema;
    using TildeSql.Schema.Columns;

    class KeyColumnValueFactory : IKeyColumnValueFactory {
        private readonly Collection collection;

        public KeyColumnValueFactory(Collection collection) {
            this.collection = collection;
        }

        public TValue GetValue<TEntity, TKey, TValue>(Column column, TEntity entity) {
            var key = this.collection.GetKey<TEntity, TKey>(entity);
            return this.GetValueUsingKey<TEntity, TKey, TValue>(column, key);
        }

        public TValue GetValueUsingKey<TEntity, TKey, TValue>(Column column, TKey key) {
            if (typeof(TKey).IsPrimitiveType()) {
                return (TValue)(dynamic)key;
            }
            
            return (TValue)key.TryGetValue(column.Name);
        }
    }
}