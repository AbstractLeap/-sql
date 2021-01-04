namespace Leap.Data.Schema {
    public record DocumentTypeColumn() : Column(typeof(string), SpecialColumns.DocumentType);
}