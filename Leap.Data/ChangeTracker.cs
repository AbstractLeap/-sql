namespace Leap.Data {
    using Leap.Data.IdentityMap;
    using Leap.Data.Internal;
    using Leap.Data.Schema;
    using Leap.Data.Serialization;

    class ChangeTracker {
        private readonly ISerializer serializer;

        private readonly ISchema schema;

        public ChangeTracker(ISerializer serializer, ISchema schema) {
            this.serializer = serializer;
            this.schema     = schema;
        }

        public bool HasEntityChanged<TEntity>(IDocument<TEntity> document) {
            if (document.State != DocumentState.Persisted) {
                return false;
            }

            var json = RowValueHelper.GetValue<string>(this.schema.GetTable<TEntity>(), document.Row.Values, SpecialColumns.Document);
            return !string.Equals(this.serializer.Serialize(document.Entity), json);
        }
    }
}