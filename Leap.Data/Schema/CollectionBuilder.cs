namespace Leap.Data.Schema {
    using System;

    public class CollectionBuilder<TEntity> {
        private readonly SchemaBuilder schemaBuilder;

        public CollectionBuilder(SchemaBuilder schemaBuilder) {
            this.schemaBuilder = schemaBuilder;
        }

        public SchemaBuilder AddComputedColumn<TColumn>(string name, string formula) {
            this.schemaBuilder.AddAction<TEntity>(collection => collection.AddComputedColumn<TColumn>(name, formula));
            return this.schemaBuilder;
        }

        public SchemaBuilder AddProjectionColumn<TColumn>(string name, Func<TEntity, TColumn> projectionFunc) {
            this.schemaBuilder.AddAction<TEntity>(collection => collection.AddProjectionColumn(name, projectionFunc));
            return this.schemaBuilder;
        }
    }
}