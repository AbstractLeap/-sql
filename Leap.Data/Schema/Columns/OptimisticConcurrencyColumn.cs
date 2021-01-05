namespace Leap.Data.Schema.Columns {
    using System;

    public record OptimisticConcurrencyColumn(Table Table) : Column(typeof(Guid), SpecialColumns.OptimisticConcurrency, Table);
}