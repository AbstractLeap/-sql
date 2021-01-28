namespace Leap.Data.Humanizer
{
    using Leap.Data.Schema;

    public static class SchemaBuilderExtensions {
        public static SchemaBuilder UseHumanizerPluralization(this SchemaBuilder schemaBuilder) {
            return schemaBuilder.UseConvention(new PluralizationConvention());
        }
    }
}
