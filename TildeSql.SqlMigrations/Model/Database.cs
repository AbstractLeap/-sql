namespace TildeSql.SqlMigrations.Model {
    using System;
    using System.Collections.Generic;

    public class Database {
        public Database() {
            this.Tables = new SortedSet<Table>(new TableComparer());
        }

        public Database(IEnumerable<Table> tables) {
            this.Tables = new SortedSet<Table>(tables, new TableComparer());
        }
        
        public ICollection<Table> Tables { get; private set; }

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
}