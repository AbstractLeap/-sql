namespace Leap.Data.Schema.Columns {
    using System;

    public record ComputedColumn : Column {
        public string Formula { get; }

        public bool Persisted { get; }

        public bool Indexed { get; }

        public ComputedColumn(Type type, Collection collection, string name, string formula, bool persisted, bool indexed)
            : base(type, name, collection) {
            this.Formula    = formula;
            this.Persisted  = persisted;
            this.Indexed    = indexed;
            this.IsComputed = true;
        }
    }
}