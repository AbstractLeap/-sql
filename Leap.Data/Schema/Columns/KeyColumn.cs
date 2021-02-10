namespace Leap.Data.Schema.Columns {
    using System;

    public record KeyColumn(Type Type, string Name, Collection Collection) : Column(Type, Name, Collection);
}