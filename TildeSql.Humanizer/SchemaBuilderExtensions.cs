namespace TildeSql.Humanizer
{
    using TildeSql.Schema;

    public static class SchemaBuilderExtensions {
        public static SchemaBuilder UseHumanizerPluralization(this SchemaBuilder schemaBuilder) {
            return schemaBuilder.UseConvention(new PluralizationConvention());
        }
    }
}
