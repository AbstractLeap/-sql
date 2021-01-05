namespace Leap.Data.Schema.Columns {
    public record DocumentColumn(Table Table) : Column(typeof(string), SpecialColumns.Document, Table);
}