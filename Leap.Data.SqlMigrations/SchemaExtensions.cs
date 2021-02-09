namespace Leap.Data.SqlMigrations {
    using System.Linq;

    using Leap.Data.Schema;
    using Leap.Data.Schema.Columns;
    using Leap.Data.Schema.Conventions.Sql;
    using Leap.Data.SqlMigrations.Model;

    using Column = Leap.Data.SqlMigrations.Model.Column;
    using Table = Leap.Data.SqlMigrations.Model.Table;

    public static class SchemaExtensions {
        public static Database ToDatabaseModel(this ISchema schema) {
            // TODO finish this
            return new Database {
                Tables = schema.All()
                               .Select(
                                   t => {
                                       return new Table {
                                           Name    = t.GetTableName(),
                                           Schema  = t.GetSchemaName(),
                                           Columns = t.Columns.Select(c => new Column { Name = c.Name, Type = c.Type, IsPrimaryKey = c is KeyColumn }).ToList()
                                       };
                                   })
                               .ToList()
            };
        }
    }
}