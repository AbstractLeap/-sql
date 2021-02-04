namespace Leap.Data.SqlMigrations.IO {
    using System.IO;
    using System.Threading.Tasks;

    public class ModelReader {
        public async ValueTask<string> ReadFileAsync(string path) {
            if (!File.Exists(path)) {
                return string.Empty;
            }

            return await File.ReadAllTextAsync(path);
        }
    }
}