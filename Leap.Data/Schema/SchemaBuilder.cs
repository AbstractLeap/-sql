namespace Leap.Data.Schema {
    using System;
    using System.Collections.Generic;
    using System.Linq;

    public class SchemaBuilder {
        private readonly List<ISchemaConvention> schemaConventions = new() { new DefaultSchemaConvention() };

        private readonly HashSet<Type> addedTypes = new();

        private readonly Dictionary<(string TableName, string CollectionName), HashSet<Type>> addedNamedTypes = new();

        private readonly IDictionary<Type, IList<Action<Table>>> buildActions = new Dictionary<Type, IList<Action<Table>>>();

        public SchemaBuilder UseConvention(ISchemaConvention convention) {
            this.schemaConventions.Add(convention);
            return this;
        }

        public SchemaBuilder AddTypes(params Type[] types) {
            this.addedTypes.UnionWith(types);
            return this;
        }

        public SchemaBuilder AddTypes(string tableName, string collectionName, params Type[] types) {
            var key = (tableName, collectionName);
            if (!this.addedNamedTypes.TryGetValue(key, out var tableTypes)) {
                tableTypes = new HashSet<Type>();
                this.addedNamedTypes.Add(key, tableTypes);
            }

            tableTypes.UnionWith(types);
            return this;
        }

        public TableBuilder<T> Setup<T>() {
            return new(this);
        }

        internal void AddAction<T>(Action<Table> action) {
            if (this.buildActions.TryGetValue(typeof(T), out var actions)) {
                actions.Add(action);
            }
            else {
                this.buildActions.Add(typeof(T), new List<Action<Table>> { action });
            }
        }

        public ISchema Build() {
            this.AddUnnamedTypesToNamed();

            var schema = new Schema();
            foreach (var namedType in this.addedNamedTypes) {
                var names = namedType.Key;
                var schemaName = this.GetConvention<ISchemaNamingSchemaConvention>().GetSchemaName(names.TableName);
                var keyType = this.GetConvention<IKeyTypeSchemaConvention>().GetKeyType(names.TableName, namedType.Value.AsEnumerable());
                var keyColumns = this.GetConvention<IKeyColumnsSchemaConvention>().GetKeyColumns(keyType);
                var table = new Table(names.CollectionName, names.TableName, schemaName, keyType, keyColumns);
                foreach (var entityType in namedType.Value) {
                    table.AddClassType(entityType);
                }

                schema.AddTable(table);
            }

            foreach (var typeActions in this.buildActions) {
                var table = schema.GetDefaultTable(typeActions.Key);
                foreach (var buildAction in typeActions.Value) {
                    buildAction(table);
                }
            }

            return schema;
        }

        private TConvention GetConvention<TConvention>()
            where TConvention : ISchemaConvention {
            for (var i = this.schemaConventions.Count - 1; i >= 0; i--) {
                if (this.schemaConventions[i] is TConvention convention) {
                    return convention;
                }
            }

            throw new Exception($"Unable to find convention of type {typeof(TConvention)}");
        }

        private void AddUnnamedTypesToNamed() {
            foreach (var type in this.addedTypes) {
                var tableName = this.GetConvention<INamingSchemaConvention>().GetTableName(type);
                var collectionName = this.GetConvention<ICollectionNamingSchemaConvention>().GetCollectionName(type);
                this.AddTypes(tableName, collectionName, type);
            }
        }
    }
}