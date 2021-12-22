namespace TildeSql.SqlMigrations {
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;

    using TildeSql.SqlMigrations.Model;

    public class Differ {
        public Difference Diff(Database currentModel, Database newModel) {
            var difference = new Difference();
            foreach (var newModelTable in newModel.Tables) {
                var matchingCurrentTable = currentModel.Tables.SingleOrDefault(t => t.Equals(newModelTable));
                if (matchingCurrentTable == null) {
                    difference.AddCreateTable(newModelTable);
                }
                else {
                    CheckColumns(difference, newModelTable, matchingCurrentTable);
                }
            }

            foreach (var currentModelTable in currentModel.Tables) {
                if (!newModel.Tables.Any(t => t.Equals(currentModelTable))) {
                    difference.AddDropTable(currentModelTable);
                }
            }

            return difference;
        }

        private void CheckColumns(Difference difference, Table newModelTable, Table matchingCurrentTable) {
            foreach (var column in newModelTable.Columns) {
                var matchingColumn = matchingCurrentTable.Columns.SingleOrDefault(c => c.Equals(column));
                if (matchingColumn == null) {
                    difference.AddCreateColumn(newModelTable, column);
                }
                else {
                    var changedProperties = new List<PropertyInfo>();
                    foreach (var propertyInfo in typeof(Column).GetProperties().Where(p => p.Name != nameof(Column.Name))) {
                        if (!Equals(propertyInfo.GetValue(column), propertyInfo.GetValue(matchingColumn))) {
                            changedProperties.Add(propertyInfo);
                        }
                    }

                    if (changedProperties.Any()) {
                        difference.AddAlterColumn(newModelTable, changedProperties, matchingColumn, column);
                    }
                }
            }

            foreach (var column in matchingCurrentTable.Columns) {
                if (!newModelTable.Columns.Any(c => c.Equals(column))) {
                    difference.AddDropColumn(newModelTable, column);
                }
            }
        }
    }
}