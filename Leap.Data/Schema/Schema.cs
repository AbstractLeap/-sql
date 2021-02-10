namespace Leap.Data.Schema {
    using System;
    using System.Collections.Generic;

    class Schema : ISchema {
        private readonly Dictionary<Type, List<Collection>> typeLookup = new();

        private readonly Dictionary<string, Collection> collectionNameLookup = new();

        private readonly Dictionary<Type, Collection> defaultCollectionLookup = new();

        public IEnumerable<Collection> All() {
            return this.collectionNameLookup.Values;
        }

        public Collection GetCollection(string collectionName) {
            if (this.collectionNameLookup.TryGetValue(collectionName, out var collection)) {
                return collection;
            }

            return null; // TODO should this throw?
        }

        public Collection GetDefaultCollection<TEntity>() {
            var entityType = typeof(TEntity);
            if (this.defaultCollectionLookup.TryGetValue(entityType, out var collection)) {
                return collection;
            }

            if (this.typeLookup.TryGetValue(entityType, out var collections)) {
                if (collections.Count == 1) {
                    this.defaultCollectionLookup.Add(entityType, collections[0]); // to speed up future requests
                    return collections[0];
                }

                if (collections.Count > 1) {
                    throw new Exception($"Unable to determine default collection for {entityType}. You should specify the default collection as you have multiple collections of this type.");
                }
            }
            
            return null; // TODO should this throw?
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