﻿namespace TildeSql.SqlMigrations {
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;

    using TildeSql.SqlMigrations.Model;

    public class Differ {
        public Difference Diff(Database currentModel, Database newModel, bool ignoreSchemaInTableNameMatching = true) {
            var difference = new Difference();
            foreach (var newModelTable in newModel.Tables) {
                var matchingCurrentTable = currentModel.Tables.SingleOrDefault(t => t.Equals(newModelTable, ignoreSchemaInTableNameMatching));
                if (matchingCurrentTable == null) {
                    difference.AddCreateTable(newModelTable);
                }
                else {
                    CheckSchema(difference, newModelTable, matchingCurrentTable);
                    CheckColumns(difference, newModelTable, matchingCurrentTable);
                    CheckIndexes(difference, newModelTable, matchingCurrentTable);
                }
            }

            foreach (var currentModelTable in currentModel.Tables) {
                if (!newModel.Tables.Any(t => t.Equals(currentModelTable, ignoreSchemaInTableNameMatching))) {
                    difference.AddDropTable(currentModelTable);
                }
            }

            return difference;
        }

        private void CheckSchema(Difference difference, Table newModelTable, Table matchingCurrentTable) {
            if (!string.Equals(newModelTable.Schema, matchingCurrentTable.Schema)) {
                difference.AddChangeSchema(matchingCurrentTable, newModelTable.Schema);
            }
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

        private void CheckIndexes(Difference difference, Table newModelTable, Table matchingCurrentTable) {
            foreach (var index in newModelTable.Indexes) {
                var matchingIndex = matchingCurrentTable.Indexes.SingleOrDefault(i => i.Name == index.Name);
                if (matchingIndex == null) {
                    difference.AddCreateIndex(newModelTable, index);
                }
                else {
                    if (index.Columns.Any(columnName => !matchingIndex.Columns.Any(matchingColumnName => columnName == matchingColumnName))
                        || matchingIndex.Columns.Any(matchingColumnName => !index.Columns.Any(columnName => columnName == matchingColumnName))) {
                        // drop and re-create index
                        difference.AddDropIndex(newModelTable, index);
                        difference.AddCreateIndex(newModelTable, index);
                    }
                }
            }

            foreach (var index in matchingCurrentTable.Indexes) {
                if (!newModelTable.Indexes.Any(i => i.Name == index.Name)) {
                    difference.AddDropIndex(newModelTable, index);
                }
            }
        }
    }
}