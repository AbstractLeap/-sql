namespace Leap.Data.SqlMigrations {
    using System.IO;
    using System.Threading.Tasks;

    using Leap.Data.Schema;
    using Leap.Data.SqlMigrations.IO;
    using Leap.Data.SqlMigrations.Model;

    public static class Migrator {
        public static async Task RunAsync(string modelPath, string migrationPath, string migrationNamespace, string migrationName, ISchema schema) {
            var currentModelJson = await new ModelReader().ReadFileAsync(modelPath);
            var currentModel = new ModelSerializer().Deserialize(currentModelJson);
            var newModel = schema.ToDatabaseModel();
            var diff = new Differ().Diff(currentModel, newModel);
            if (diff.IsChange) {
                var migrationCode = new Generator().CreateCode(diff, migrationNamespace, migrationName);
                await File.WriteAllTextAsync(migrationPath, migrationCode);
                await new ModelWriter().WriteFileAsync(modelPath, new ModelSerializer().Serialize(newModel));
            }
        }
    }
}