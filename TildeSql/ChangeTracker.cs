namespace TildeSql {
    using TildeSql.IdentityMap;
    using TildeSql.Internal;
    using TildeSql.Schema;
    using TildeSql.Serialization;

    class ChangeTracker {
        private readonly ISerializer serializer;

        private readonly ISchema schema;

        private readonly IChangeDetector changeDetector;

        public ChangeTracker(ISerializer serializer, ISchema schema, IChangeDetector changeDetector) {
            this.serializer     = serializer;
            this.schema         = schema;
            this.changeDetector = changeDetector ?? new DefaultChangeDetector(serializer);
        }

        public bool HasEntityChanged(IDocument document) {
            if (document.State != DocumentState.Persisted) {
                return false;
            }

            var json = RowValueHelper.GetValue<string>(document.Collection, document.Row.Values, SpecialColumns.Document);
            return this.changeDetector.HasChanged(json, document.GetEntity());
        }
    }
}