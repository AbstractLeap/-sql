namespace Leap.Data.Schema.Columns {
    public record DocumentTypeColumn(Table Table) : Column(typeof(string), SpecialColumns.DocumentType, Table);
}