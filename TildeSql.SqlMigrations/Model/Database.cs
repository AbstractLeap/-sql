namespace TildeSql.SqlMigrations.Model {
    using System.Collections.Generic;

    public class Database {
        public Database() {
            this.Tables = new List<Table>();
        }
        
        public ICollection<Table> Tables { get; set; }
    }
}