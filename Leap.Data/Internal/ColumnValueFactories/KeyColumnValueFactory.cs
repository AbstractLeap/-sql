namespace Leap.Data.Internal.ColumnValueFactories {
    using Fasterflect;

    using Leap.Data.Schema;
    using Leap.Data.Schema.Columns;
    using Leap.Data.Utilities;

    class KeyColumnValueFactory : IKeyColumnValueFactory {
        private readonly Collection collection;

        public KeyColumnValueFactory(Collection collection) {
            this.collection = collection;
        }

        public TValue GetValue<TEntity, TKey, TValue>(Column column, TEntity entity) {
            var key = (TKey)this.collection.KeyMember.Get(entity);
            return this.GetValueUsingKey<TEntity, TKey, TValue>(column, key);
        }

        public TValue GetValueUsingKey<TEntity, TKey, TValue>(Column column, TKey key) {
            if (typeof(TKey).IsPrimitiveKeyType()) {
                return (TValue)(dynamic)key;
            }
            
            return (TValue)key.TryGetValue(column.Name);
        }
    }
}