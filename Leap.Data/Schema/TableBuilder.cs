namespace Leap.Data.Schema {
    using System;

    public class TableBuilder<TEntity> {
        private readonly SchemaBuilder schemaBuilder;

        public TableBuilder(SchemaBuilder schemaBuilder) {
            this.schemaBuilder = schemaBuilder;
        }

        public SchemaBuilder AddComputedColumn<TColumn>(string name, string formula) {
            this.schemaBuilder.AddAction<TEntity>(table => table.AddComputedColumn<TColumn>(name, formula));
            return this.schemaBuilder;
        }

        public SchemaBuilder AddProjectionColumn<TColumn>(string name, Func<TEntity, TColumn> projectionFunc) {
            this.schemaBuilder.AddAction<TEntity>(table => table.AddProjectionColumn(name, projectionFunc));
            return this.schemaBuilder;
        }
    }
}