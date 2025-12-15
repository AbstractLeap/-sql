namespace TildeSql.SqlMigrations {
    using System.Collections.Generic;
    using System.Linq;

    using TildeSql.Schema;
    using TildeSql.Schema.Columns;
    using TildeSql.Schema.Conventions.Sql;
    using TildeSql.SqlMigrations.Model;

    using Column = TildeSql.SqlMigrations.Model.Column;

    public static class SchemaExtensions {
        public static Database ToDatabaseModel(this ISchema schema) {
            // TODO finish this
            return new Database (schema.All()
                               .Select(
                                   t => {
                                       return new Table {
                                           Name   = t.GetTableName(),
                                           Schema = t.GetSchemaName(),
                                           Columns = t.Columns.Select(
                                                          c => new Column {
                                                              Name            = c.Name,
                                                              Type            = c is DocumentColumn ? typeof(Json) : c.Type,
                                                              IsPrimaryKey    = c is KeyColumn,
                                                              IsIdentity      = (c is KeyColumn && t.IsKeyComputed) || c is GenericColumn { IsIdentity: true },
                                                              IsComputed      = c is ComputedColumn or GenericColumn { IsComputed: true } or GenericColumn { IsIdentity: true },
                                                              IsPersisted     = c is ComputedColumn { Persisted: true },
                                                              ComputedFormula = c is ComputedColumn computed ? computed.Formula : null,
                                                              Size = c switch {
                                                                  DocumentTypeColumn => 255,
                                                                  GenericColumn genericColumn => genericColumn.Size,
                                                                  _ => null
                                                              },
                                                              Precision  = c is GenericColumn genericColumn1 ? genericColumn1.Precision : null,
                                                              IsNullable = c is GenericColumn { IsNullable: true }
                                                          })
                                                      .ToList(),
                                           Indexes = t.Columns.OfType<ComputedColumn>()
                                                      .Select(c => new Index { Name = $"idx_{c.Name}", Columns = new List<string> { c.Name } })
                                                      .ToList()
                                       };
                                   }));
        }
    }
}