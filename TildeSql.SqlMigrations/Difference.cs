namespace TildeSql.SqlMigrations {
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;

    using TildeSql.SqlMigrations.Model;

    public class Difference {
        private readonly IList<Table> createTables = new List<Table>();

        private readonly IList<Table> dropTables = new List<Table>();

        private readonly IList<(Table Table, Column Column)> createColumns = new List<(Table Table, Column Column)>();

        private readonly IList<(Table Table, Column Column)> dropColumns = new List<(Table Table, Column Column)>();

        private readonly IList<(Table Table, Column OldColumn, Column NewColumn, IList<PropertyInfo> ChangedProperties)> alterColumns =
            new List<(Table Table, Column OldColumn, Column NewColumn, IList<PropertyInfo> ChangedProperties)>();

        public IEnumerable<Table> CreateTables => this.createTables;

        public IEnumerable<(Table Table, Column Column)> CreateColumns => this.createColumns;

        public IEnumerable<(Table Table, Column OldColumn, Column NewColumn, IList<PropertyInfo> ChangedProperties)> AlterColumns => this.alterColumns;

        public IEnumerable<(Table Table, Column Column)> DropColumns => this.dropColumns;

        public IEnumerable<Table> DropTables => this.dropTables;

        public bool IsChange => this.createTables.Any() || this.createColumns.Any() || this.alterColumns.Any() || this.dropColumns.Any() || this.dropTables.Any();

        public void AddCreateTable(Table table) {
            this.createTables.Add(table);
        }

        public void AddCreateColumn(Table table, Column column) {
            this.createColumns.Add((table, column));
        }

        public void AddDropTable(Table table) {
            this.dropTables.Add(table);
        }

        public void AddDropColumn(Table table, Column column) {
            this.dropColumns.Add((table, column));
        }

        public void AddAlterColumn(Table table, IList<PropertyInfo> changedProperties, Column? oldColumn, Column newColumn) {
            this.alterColumns.Add((table, oldColumn, newColumn, changedProperties));
        }
    }
}