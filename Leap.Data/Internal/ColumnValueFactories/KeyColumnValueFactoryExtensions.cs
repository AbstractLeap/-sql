namespace Leap.Data.Internal.ColumnValueFactories {
    using Fasterflect;

    using Leap.Data.Schema.Columns;

    static class KeyColumnValueFactoryExtensions {
        public static object GetValue<TEntity, TKey>(this IKeyColumnValueFactory keyColumnValueFactory, Column column, TKey key) {
            return keyColumnValueFactory.CallMethod(
                new[] { typeof(TEntity), typeof(TKey), column.Type }
                , nameof(IKeyColumnValueFactory.GetValueUsingKey),
                new [] { typeof(Column), typeof(TKey) },
                column, key);
        }
    }
}