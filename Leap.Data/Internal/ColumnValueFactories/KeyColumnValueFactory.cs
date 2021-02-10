namespace Leap.Data.Internal.ColumnValueFactories {
    using Fasterflect;

    using Leap.Data.Schema;
    using Leap.Data.Schema.Columns;

    class KeyColumnValueFactory : IKeyColumnValueFactory {
        private readonly Collection collection;

        public KeyColumnValueFactory(Collection collection) {
            this.collection = collection;
        }

        public TValue GetValue<TEntity, TKey, TValue>(Column column, TEntity entity) {
            var key = this.collection.KeyExtractor.Extract<TEntity, TKey>(entity);
            return this.GetValueUsingKey<TEntity, TKey, TValue>(column, key);
        }

        public TValue GetValueUsingKey<TEntity, TKey, TValue>(Column column, TKey key) {
            return (TValue)key.TryGetValue(column.Name);
        }
    }
}