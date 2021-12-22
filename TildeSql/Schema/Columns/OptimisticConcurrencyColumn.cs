namespace TildeSql.Schema.Columns {
    using System;

    public record OptimisticConcurrencyColumn(Collection Collection) : Column(typeof(Guid), SpecialColumns.OptimisticConcurrency, Collection);
}