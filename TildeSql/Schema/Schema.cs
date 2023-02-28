namespace TildeSql.Schema {
    using System;
    using System.Collections.Generic;

    class Schema : ISchema {
        private readonly Dictionary<Type, List<Collection>> typeLookup = new();

        private readonly Dictionary<string, Collection> collectionNameLookup = new();

        private readonly Dictionary<Type, Collection> defaultCollectionLookup = new();

        public IEnumerable<Collection> All() {
            return this.collectionNameLookup.Values;
        }

        public IEnumerable<Collection> GetCollections<TEntity>() {
            return this.GetCollections(typeof(TEntity));
        }

        public IEnumerable<Collection> GetCollections(Type entityType) {
            return this.typeLookup[entityType].AsReadOnly();
        }

        public Collection GetCollection<TEntity>(string collectionName) {
            if (string.IsNullOrWhiteSpace(collectionName)) {
                return this.GetDefaultCollection<TEntity>();
            }

            if (this.collectionNameLookup.TryGetValue(collectionName, out var collection)) {
                return collection;
            }

            throw new ArgumentOutOfRangeException(nameof(collectionName), $"Unable to find a collection named {collectionName}");
        }

        public Collection GetDefaultCollection<TEntity>() {
            var collectionType = typeof(TEntity);
            if (collectionType.IsGenericType) {
                collectionType = collectionType.GetGenericTypeDefinition();
            }

            if (this.defaultCollectionLookup.TryGetValue(collectionType, out var collection)) {
                return collection;
            }

            if (this.typeLookup.TryGetValue(collectionType, out var collections)) {
                if (collections.Count == 1) {
                    this.defaultCollectionLookup.TryAdd(collectionType, collections[0]); // to speed up future requests
                    return collections[0];
                }

                if (collections.Count > 1) {
                    throw new Exception(
                        $"Unable to determine default collection for {collectionType}. You should specify the default collection as you have multiple collections of this type.");
                }
            }

            throw new Exception($"Unable to determine the default collection for type {collectionType}");
        }

        public void SetDefaultCollectionName(Type entityType, Collection defaultCollection) {
            this.defaultCollectionLookup[entityType] = defaultCollection;
        }

        public void AddCollection(Collection collection) {
            if (this.collectionNameLookup.ContainsKey(collection.CollectionName)) {
                throw new Exception($"A collection for the collection {collection.CollectionName} already exists");
            }
            
            this.collectionNameLookup.Add(collection.CollectionName, collection);
            foreach (var entityType in collection.EntityTypes) {
                if (!this.typeLookup.TryGetValue(entityType, out var collections)) {
                    collections = new List<Collection> { collection };
                    this.typeLookup.Add(entityType, collections);
                }
                else {
                    collections.Add(collection);
                }
            }
        }
    }
}