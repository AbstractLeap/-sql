namespace Leap.Data.Internal.ColumnValueFactories {
    using Leap.Data.Schema.Columns;

    class DocumentTypeColumnValueFactory : IColumnValueFactory {
        public TValue GetValue<TEntity, TKey, TValue>(Column column, TEntity entity) {
            return (TValue)(object)entity.GetType().AssemblyQualifiedName;
        }
    }
}