namespace TildeSql.SqlMigrations.Model {
    using System.Text.Json;

    public class ModelSerializer {
        public string Serialize(Database database) {
            var options = new JsonSerializerOptions { WriteIndented = true };
            options.Converters.Add(new CustomJsonConverterForType());
            return JsonSerializer.Serialize(database, options);
        }

        public Database Deserialize(string json) {
            if (string.IsNullOrWhiteSpace(json)) {
                return new Database();
            }

            var options = new JsonSerializerOptions { WriteIndented = true };
            options.Converters.Add(new CustomJsonConverterForType());
            return JsonSerializer.Deserialize<Database>(json, options);
        }
    }
}