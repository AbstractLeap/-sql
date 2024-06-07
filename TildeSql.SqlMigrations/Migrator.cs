namespace TildeSql.SqlMigrations {
    using System;
    using System.IO;
    using System.Threading.Tasks;

    using TildeSql.Schema;
    using TildeSql.SqlMigrations.IO;
    using TildeSql.SqlMigrations.Model;

    public static class Migrator {
        public static async Task RunAsync(
            string modelPath,
            string migrationPath,
            string migrationNamespace,
            string migrationName,
            ISchema schema,
            Func<(string TableName, string SchemaName), bool> dropAndRecreateFilter = null,
            bool ignoreSchemaInTableNameMatching = true,
            string[] tags = null) {
            var currentModelJson = await new ModelReader().ReadFileAsync(modelPath);
            var currentModel = new ModelSerializer().Deserialize(currentModelJson);
            var newModel = schema.ToDatabaseModel();
            var diff = new Differ().Diff(currentModel, newModel, ignoreSchemaInTableNameMatching);
            if (diff.IsChange) {
                var migrationCode = new Generator().CreateCode(diff, migrationNamespace, migrationName, dropAndRecreateFilter, tags);
                await File.WriteAllTextAsync(migrationPath, migrationCode);
                await new ModelWriter().WriteFileAsync(modelPath, new ModelSerializer().Serialize(newModel));
            }
        }
    }
}