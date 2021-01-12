namespace Leap.Data.Internal.ColumnValueFactories {
    using System;

    using Leap.Data.IdentityMap;
    using Leap.Data.Schema.Columns;

    class OptimisticConcurrencyColumnValueFactory : IColumnValueFactory {
        public TValue GetValue<TEntity, TKey, TValue>(Column column, TEntity entity, IDocument<TEntity> document) {
            return (TValue)(object)Guid.NewGuid();
        }
    }
}