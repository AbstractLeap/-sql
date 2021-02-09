namespace Leap.Data.SqlServer {
    using Leap.Data.Schema;
    using Leap.Data.Schema.Conventions.Sql;

    public static class SchemaBuilderExtensions {
        public static SchemaBuilder UseSqlServerConvention(this SchemaBuilder builder) {
            builder.UseConvention(new DefaultSqlSchemaConvention());
            builder.UseConvention(new SqlStorageSchemaConvention(builder));
            return builder;
        }
    }
}