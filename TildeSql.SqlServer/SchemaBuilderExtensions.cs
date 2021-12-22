namespace TildeSql.SqlServer {
    using TildeSql.Schema;
    using TildeSql.Schema.Conventions.Sql;

    public static class SchemaBuilderExtensions {
        public static SchemaBuilder UseSqlServerConvention(this SchemaBuilder builder) {
            builder.UseConvention(new DefaultSqlSchemaConvention());
            builder.UseConvention(new SqlStorageSchemaConvention(builder));
            return builder;
        }
    }
}