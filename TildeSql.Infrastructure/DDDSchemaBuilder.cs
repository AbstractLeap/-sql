namespace TildeSql.Infrastructure {
    using TildeSql.Schema;
    using TildeSql.Humanizer;
    using TildeSql.SqlServer;
    using TildeSql.Schema.Conventions;

    public abstract class DDDSchemaBuilder {
        private readonly SchemaBuilder schemaBuilder;

        public DDDSchemaBuilder() {
            this.schemaBuilder = new SchemaBuilder()
                                .AddTypes("DomainEvents", typeof(DomainEventWrapper), typeof(DomainEventWrapper<>))
                                .UseHumanizerPluralization()
                                .UseSqlServerConvention();
            DefaultSchemaConventions = new List<ISchemaConvention> {
                new CollectionNamingSchemaConvention(),
                                new SchemaNameSchemaConvention(),
                                new OptimisticConcurrencySchemaConvention(),
                                new KeyComputedSchemaConvention()
                };

            ConfigureSchemaBuilder(schemaBuilder);

            foreach (var convention in DefaultSchemaConventions) {
                schemaBuilder.UseConvention(convention);
            }
        }

        protected List<ISchemaConvention> DefaultSchemaConventions { get; }

        protected abstract void ConfigureSchemaBuilder(SchemaBuilder schemaBuilder);

        public ISchema Build() => this.schemaBuilder.Build();
    }
}