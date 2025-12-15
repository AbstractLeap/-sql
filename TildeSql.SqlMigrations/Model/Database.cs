namespace TildeSql.SqlMigrations.Model {
    using System.Collections.Generic;

    public class Database {
        public Database() {
            this.Tables = new SortedSet<Table>(new TableComparer());
        }

        public Database(IEnumerable<Table> tables) {
            this.Tables = new SortedSet<Table>(tables, new TableComparer());
        }

        public SortedSet<Table> Tables { get; set; }
    }
}