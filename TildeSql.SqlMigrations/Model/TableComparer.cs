namespace TildeSql.SqlMigrations.Model {
    using System;
    using System.Collections.Generic;

    class TableComparer : IComparer<Table> {
        public int Compare(Table x, Table y) {
            if (ReferenceEquals(x, y)) return 0;
            if (x is null) return -1;
            if (y is null) return 1;
            if (x.Schema == y.Schema) return string.Compare(x.Name, y.Name, StringComparison.Ordinal);
            return string.Compare(x.Schema, y.Schema, StringComparison.Ordinal);
        }
    }
}