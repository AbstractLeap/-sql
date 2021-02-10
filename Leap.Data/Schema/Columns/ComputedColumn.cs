namespace Leap.Data.Schema.Columns {
    public record ComputedColumn<T> : Column {
        public string Formula { get; init; }

        public ComputedColumn(Collection collection, string name, string formula)
            : base(typeof(T), name, collection) {
            this.Formula    = formula;
            this.IsComputed = true;
        }
    }
}