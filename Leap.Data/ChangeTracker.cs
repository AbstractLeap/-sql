namespace Leap.Data {
    using Leap.Data.IdentityMap;
    using Leap.Data.Schema;
    using Leap.Data.Serialization;

    class ChangeTracker {
        private readonly ISerializer serializer;

        public ChangeTracker(ISerializer serializer) {
            this.serializer = serializer;
        }

        public bool HasEntityChanged<TEntity>(Document<TEntity> document) {
            if (document.State != DocumentState.Persisted) {
                return false;
            }

            var json = document.Row.GetValue<string>(SpecialColumns.Document);
            return !string.Equals(this.serializer.Serialize(document.Entity), json);
        }
    }
}