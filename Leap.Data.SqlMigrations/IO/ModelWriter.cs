namespace Leap.Data.SqlMigrations.IO {
    using System.IO;
    using System.Threading.Tasks;

    public class ModelWriter {
        public async ValueTask WriteFileAsync(string path, string contents) {
            await File.WriteAllTextAsync(path, contents);
        }
    }
}