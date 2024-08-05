namespace TildeSql.Schema.Columns {
    public record DocumentColumn(Collection Collection) : Column(typeof(Json), SpecialColumns.Document, Collection);
}