namespace TildeSql.Internal.ColumnValueFactories {
    using TildeSql.Schema.Columns;
    using TildeSql.Serialization;

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