namespace Leap.Data.Schema {
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using Leap.Data.Schema.Conventions;

    public class SchemaBuilder {
        private readonly List<ISchemaConvention> schemaConventions = new() { new DefaultSchemaConvention() };

        private readonly HashSet<Type> addedTypes = new();

        private readonly Dictionary<string, (bool IsDefault, HashSet<Type> Types)> addedNamedTypes = new();

        private readonly IDictionary<Type, IList<Action<Collection>>> buildActions = new Dictionary<Type, IList<Action<Collection>>>();

        public SchemaBuilder UseConvention(ISchemaConvention convention) {
            this.schemaConventions.Add(convention);
            return this;
        }

        public SchemaBuilder AddTypes(params Type[] types) {
            this.addedTypes.UnionWith(types);
            return this;
        }

        public SchemaBuilder AddTypes(string collectionName, params Type[] types) {
            return this.AddTypes(collectionName, false, types);
        }

        private SchemaBuilder AddTypes(string collectionName, bool isDefault, params Type[] types) {
            if (!this.addedNamedTypes.TryGetValue(collectionName, out var collectionTypes)) {
                collectionTypes = (isDefault, new HashSet<Type>());
                this.addedNamedTypes.Add(collectionName, collectionTypes);
            }

            collectionTypes.Types.UnionWith(types);
            return this;
        }

        private void AddUnnamedTypesToNamed() {
            foreach (var type in this.addedTypes) {
                var collectionName = this.GetConvention<ICollectionNamingSchemaConvention>().GetCollectionName(type);
                this.AddTypes(collectionName, true, type);
            }
        }

        public CollectionBuilder<T> Setup<T>() {
            return new(this);
        }

        internal void AddAction<T>(Action<Collection> action) {
            if (this.buildActions.TryGetValue(typeof(T), out var actions)) {
                actions.Add(action);
            }
            else {
                this.buildActions.Add(typeof(T), new List<Action<Collection>> { action });
            }
        }

        public ISchema Build() {
            this.AddUnnamedTypesToNamed();

            var schema = new Schema();
            foreach (var namedType in this.addedNamedTypes) {
                var collectionName = namedType.Key;
                var keyMember = this.GetConvention<IKeyMemberSchemaConvention>().GetKeyMember(collectionName, namedType.Value.Types.AsEnumerable());
                var useOptimisticConcurrency = this.GetConvention<IOptimisticConcurrencySchemaConvention>()
                                                   .UseOptimisticConcurrency(collectionName, namedType.Value.Types.AsEnumerable());
                var isKeyComputed = this.GetConvention<IKeyComputedSchemaConvention>().IsKeyComputed(collectionName, namedType.Value.Types.AsEnumerable());
                var storageSettings = this.GetConvention<IStorageSchemaConvention>().Configure(collectionName, namedType.Value.Types);
                var collection = new Collection(collectionName, keyMember, useOptimisticConcurrency, isKeyComputed) { StorageSettings = storageSettings };

                foreach (var entityType in namedType.Value.Types) {
                    collection.AddClassType(entityType);
                }

                schema.AddCollection(collection);
                if (namedType.Value.IsDefault) {
                    foreach (var entityType in namedType.Value.Types) {
                        schema.SetDefaultCollectionName(entityType, collection);
                    }
                }
            }

            foreach (var typeActions in this.buildActions) {
                var collection = schema.GetDefaultCollection(typeActions.Key);
                foreach (var buildAction in typeActions.Value) {
                    buildAction(collection);
                }
            }

            return schema;
        }

        internal TConvention GetConvention<TConvention>()
            where TConvention : ISchemaConvention {
            for (var i = this.schemaConventions.Count - 1; i >= 0; i--) {
                if (this.schemaConventions[i] is TConvention convention) {
                    return convention;
                }
            }

            throw new Exception($"Unable to find convention of type {typeof(TConvention)}");
        }
    }
}