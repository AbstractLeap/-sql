namespace Leap.Data.Schema {
    using System;
    using System.Collections.Generic;
    using System.Linq;

    public class SchemaBuilder {
        private ISchemaConvention schemaConvention;

        private readonly HashSet<Type> addedTypes = new();

        private readonly Dictionary<string, HashSet<Type>> addedNamedTypes = new();

        public SchemaBuilder() {
            this.schemaConvention = new DefaultSchemaConvention();
        }

        public SchemaBuilder UsingConvention(ISchemaConvention convention) {
            this.schemaConvention = convention;
            return this;
        }

        public SchemaBuilder AddTypes(params Type[] types) {
            this.addedTypes.UnionWith(types);
            return this;
        }

        public SchemaBuilder AddTypes(string tableName, params Type[] types) {
            if (!this.addedNamedTypes.TryGetValue(tableName, out var tableTypes)) {
                tableTypes = new HashSet<Type>();
                this.addedNamedTypes.Add(tableName, tableTypes);
            }
            
            tableTypes.UnionWith(types);
            return this;
        }

        public ISchema Build() {
            this.AddUnnamedTypesToNamed();

            var schema = new Schema();
            foreach (var namedType in this.addedNamedTypes) {
                var tableName = namedType.Key;
                var schemaName = this.schemaConvention.GetSchemaName(tableName);
                var keyType = this.schemaConvention.GetKeyType(tableName, namedType.Value.AsEnumerable());
                var keyColumns = this.schemaConvention.GetKeyColumns(keyType);
                var table = new Table(tableName, schemaName, keyType, keyColumns);
                foreach (var entityType in namedType.Value) {
                    table.AddClassType(entityType);
                }

                schema.AddTable(table);
            }

            return schema;
        }

        private void AddUnnamedTypesToNamed() {
            foreach (var type in this.addedTypes) {
                var tableName = this.schemaConvention.GetTableName(type);
                this.AddTypes(tableName, type);
            }
        }
    }
}