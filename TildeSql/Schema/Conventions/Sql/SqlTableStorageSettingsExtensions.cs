namespace TildeSql.Schema.Conventions.Sql {
    using System;

    public static class SqlTableStorageSettingsExtensions {
        public static string GetTableName(this Collection collection) {
            return GetSetting(collection.StorageSettings, settings => settings.TableName);
        }

        public static string GetSchemaName(this Collection collection) {
            return GetSetting(collection.StorageSettings, settings => settings.SchemaName);
        }

        private static TResult GetSetting<TResult>(this ICollectionStorageSettings collectionStorageSettings, Func<SqlCollectionStorageSettings, TResult> accessor) {
            if (collectionStorageSettings is SqlCollectionStorageSettings sqlTableStorageSettings) {
                return accessor(sqlTableStorageSettings);
            }

            throw new Exception("Unable to get sql storage settings. Did you forget to set up the schema builder for using SQL based storage?");
        }
    }
}