namespace Leap.Data.Schema.Columns {
    using System;

    public record KeyColumn(Type Type, string Name, Table Table) : Column(Type, Name, Table);
}