namespace Leap.Data.SqlMigrations.Model {
    using System.Collections.Generic;

    public class Index {
        public string Name { get; set; }

        public List<string> Columns { get; set; }
    }
}