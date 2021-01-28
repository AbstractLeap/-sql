namespace Leap.Data.Internal.ColumnValueFactories {
    using Leap.Data.IdentityMap;
    using Leap.Data.Schema.Columns;

    class NullColumnFactory : IColumnValueFactory {
        public TValue GetValue<TEntity, TKey, TValue>(Column column, TEntity entity, IDocument<TEntity> document) {
            return default;
        }
    }
}