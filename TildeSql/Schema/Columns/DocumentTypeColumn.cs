﻿namespace TildeSql.Schema.Columns {
    public record DocumentTypeColumn(Collection Collection) : Column(typeof(string), SpecialColumns.DocumentType, Collection);
}