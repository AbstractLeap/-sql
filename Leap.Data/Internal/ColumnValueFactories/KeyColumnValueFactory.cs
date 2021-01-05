namespace Leap.Data.Internal.ColumnValueFactories {
    using Fasterflect;

    using Leap.Data.IdentityMap;
    using Leap.Data.Schema;
    using Leap.Data.Schema.Columns;

    class KeyColumnValueFactory : IKeyColumnValueFactory {
        private readonly Table table;

        public KeyColumnValueFactory(Table table) {
            this.table = table;
        }

        public TValue GetValue<TEntity, TKey, TValue>(Column column, TEntity entity, Document<TEntity> document) {
            var key = this.table.KeyExtractor.Extract<TEntity, TKey>(entity);
            return this.GetValue<TEntity, TKey, TValue>(column, key);
        }

        public TValue GetValue<TEntity, TKey, TValue>(Column column, TKey key) {
            return (TValue)key.TryGetValue(column.Name);
        }
    }
}