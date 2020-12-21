namespace Leap.Data.Internal {
    using Fasterflect;

    using Leap.Data.Schema;

    class KeyExtractor {
        private readonly ISchema schema;

        public KeyExtractor(ISchema schema) {
            this.schema = schema;
        }

        public TKey Extract<TEntity, TKey>(TEntity entity) {
            return (TKey)this.schema.GetTable<TEntity>().KeyMember.Get(entity);
        }
    }
}