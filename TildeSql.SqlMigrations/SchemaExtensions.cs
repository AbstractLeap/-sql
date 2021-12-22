namespace TildeSql.SqlMigrations {
    using System.Collections.Generic;
    using System.Linq;

    using TildeSql.Schema.Conventions.Sql;
    using TildeSql.Schema;
    using TildeSql.Schema.Columns;
    using TildeSql.SqlMigrations.Model;

    using Column = TildeSql.SqlMigrations.Model.Column;
    using Table = TildeSql.SqlMigrations.Model.Table;

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
                                           Columns = t.Columns.Select(c => new Column {
                                               Name            = c.Name, 
                                               Type            = c.Type, 
                                               IsPrimaryKey    = c is KeyColumn, 
                                               IsIdentity      = t.IsKeyComputed,
                                               IsComputed      = c is ComputedColumn,
                                               IsPersisted     = c is ComputedColumn { Persisted: true },
                                               ComputedFormula = c is ComputedColumn computed ? computed.Formula : null
                                           }).ToList(),
                                           Indexes = t.Columns.OfType<ComputedColumn>().Select(c => new Index {
                                               Name = $"idx_{c.Name}",
                                               Columns = new List<string> { c.Name }
                                           }).ToList()
                                       };
                                   })
                               .ToList()
            };
        }
    }
}