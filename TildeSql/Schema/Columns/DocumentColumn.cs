namespace TildeSql.Schema.Columns {
    public record DocumentColumn(Collection Collection) : Column(typeof(string), SpecialColumns.Document, Collection);
}