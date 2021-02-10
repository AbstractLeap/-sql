namespace Leap.Data.Schema.Columns {
    public record DocumentTypeColumn(Collection Collection) : Column(typeof(string), SpecialColumns.DocumentType, Collection);
}