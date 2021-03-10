namespace Leap.Data.Schema.Conventions {
    using System;
    using System.Collections.Generic;

    public class DefaultSchemaConvention : IStorageSchemaConvention,
                                           IOptimisticConcurrencySchemaConvention,
                                           ICollectionNamingSchemaConvention,
                                           IKeyTypeSchemaConvention,
                                           IKeyColumnsSchemaConvention,
                                           IKeyComputedSchemaConvention {
        public virtual Type GetKeyType(string collectionName, IEnumerable<Type> entityTypes) {
            return new DefaultKeyTypeExtractor().Extract(collectionName, entityTypes);
        }

        public virtual IEnumerable<(Type Type, string Name)> GetKeyColumns(Type keyType) {
            return new DefaultKeyColumnExtractor().Extract(keyType);
        }

        public virtual string GetCollectionName(Type type) {
            if (type.IsGenericTypeDefinition) {
                var backTickIdx = type.Name.IndexOf('`');
                return backTickIdx > -1 ? type.Name.Remove(backTickIdx) : type.Name;
            }

            return type.Name;
        }

        public ICollectionStorageSettings Configure(string collectionName, HashSet<Type> types) {
            return null;
        }

        public bool UseOptimisticConcurrency(string collectionName, IEnumerable<Type> entityTypes) {
            return true;
        }

        public bool IsKeyComputed(string collectionName, IEnumerable<Type> entityTypes) {
            return false;
        }
    }
}