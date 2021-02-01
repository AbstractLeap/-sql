namespace Leap.Data.Internal.ColumnValueFactories {
    using System;

    using Leap.Data.Schema.Columns;

    class OptimisticConcurrencyColumnValueFactory : IColumnValueFactory {
        public TValue GetValue<TEntity, TKey, TValue>(Column column, TEntity entity) {
            return (TValue)(object)Guid.NewGuid();
        }
    }
}