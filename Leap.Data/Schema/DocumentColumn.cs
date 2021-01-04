namespace Leap.Data.Schema {
    public record DocumentColumn() : Column(typeof(string), SpecialColumns.Document);
}