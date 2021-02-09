namespace Leap.Data.Schema.Conventions.Sql {
    using System;

    public static class SqlTableStorageSettingsExtensions {
        public static string GetTableName(this Table table) {
            return GetSetting(table.StorageSettings, settings => settings.TableName);
        }

        public static string GetSchemaName(this Table table) {
            return GetSetting(table.StorageSettings, settings => settings.SchemaName);
        }

        private static TResult GetSetting<TResult>(this ITableStorageSettings tableStorageSettings, Func<SqlTableStorageSettings, TResult> accessor) {
            if (tableStorageSettings is SqlTableStorageSettings sqlTableStorageSettings) {
                return accessor(sqlTableStorageSettings);
            }

            throw new Exception("Unable to get sql storage settings. Did you forget to set up the schema builder for using SQL based storage?");
        }
    }
}