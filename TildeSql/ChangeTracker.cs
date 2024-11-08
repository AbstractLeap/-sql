namespace TildeSql {
    using TildeSql.IdentityMap;
    using TildeSql.Internal;
    using TildeSql.Schema;
    using TildeSql.Serialization;

    class ChangeTracker {
        private readonly ISerializer serializer;

        private readonly ISchema schema;

        public ChangeTracker(ISerializer serializer, ISchema schema) {
            this.serializer = serializer;
            this.schema     = schema;
        }

        public bool HasEntityChanged(IDocument document) {
            if (document.State != DocumentState.Persisted) {
                return false;
            }

            var json = RowValueHelper.GetValue<string>(document.Collection, document.Row.Values, SpecialColumns.Document);
            return !JsonSolidusEscapeIgnoringStringComparator.StringEquals(json, this.serializer.Serialize(document.GetEntity()));
        }
    }
}