namespace Leap.Data.Schema.Columns {
    public record ComputedColumn<T> : Column {
        public string Formula { get; init; }

        public ComputedColumn(Table table, string name, string formula)
            : base(typeof(T), name, table) {
            this.Formula    = formula;
            this.IsComputed = true;
        }
    }
}