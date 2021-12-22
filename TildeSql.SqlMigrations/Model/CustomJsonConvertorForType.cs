namespace TildeSql.SqlMigrations.Model {
    using System;
    using System.Text.Json;
    using System.Text.Json.Serialization;

    internal class CustomJsonConverterForType : JsonConverter<Type> {
        public override Type Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) {
            string assemblyQualifiedName = reader.GetString();
            return Type.GetType(assemblyQualifiedName);
        }

        public override void Write(Utf8JsonWriter writer, Type value, JsonSerializerOptions options) {
            writer.WriteStringValue(value.AssemblyQualifiedName);
        }
    }
}