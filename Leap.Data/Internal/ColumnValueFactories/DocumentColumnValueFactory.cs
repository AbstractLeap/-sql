namespace Leap.Data.Internal.ColumnValueFactories {
    using Leap.Data.Schema.Columns;
    using Leap.Data.Serialization;

    class DocumentColumnValueFactory : IColumnValueFactory {
        private readonly ISerializer serializer;

        public DocumentColumnValueFactory(ISerializer serializer) {
            this.serializer = serializer;
        }

        public TValue GetValue<TEntity, TKey, TValue>(Column column, TEntity entity) {
            return (TValue)(object)this.serializer.Serialize(entity);
        }
    }
}