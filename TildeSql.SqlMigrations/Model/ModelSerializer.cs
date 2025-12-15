namespace TildeSql.SqlMigrations.Model {
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text.Json;
    using System.Text.Json.Serialization;

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

            var options = new JsonSerializerOptions { WriteIndented = true, Converters = { new SortedSetWithComparerConverter() } };
            options.Converters.Add(new CustomJsonConverterForType());
            return JsonSerializer.Deserialize<Database>(json, options);
        }

        private sealed class SortedSetWithComparerConverter : JsonConverter<SortedSet<Table>> {
            private readonly TableComparer comparer = new();

            public override SortedSet<Table> Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) {
                var list = JsonSerializer.Deserialize<List<Table>>(ref reader, options) ?? new List<Table>();
                return new SortedSet<Table>(list, this.comparer);
            }

            public override void Write(Utf8JsonWriter writer, SortedSet<Table> value, JsonSerializerOptions options) {
                // Serialize as an array of items for compactness
                JsonSerializer.Serialize(writer, value.ToArray(), options);
            }
        }
    }
}