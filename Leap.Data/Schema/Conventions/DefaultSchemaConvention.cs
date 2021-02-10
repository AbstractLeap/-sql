namespace Leap.Data.Schema.Conventions {
    using System;
    using System.Collections.Generic;

    public class DefaultSchemaConvention : IStorageSchemaConvention, ICollectionNamingSchemaConvention, IKeyTypeSchemaConvention, IKeyColumnsSchemaConvention {
        public virtual Type GetKeyType(string collectionName, IEnumerable<Type> entityTypes) {
            return new DefaultKeyTypeExtractor().Extract(collectionName, entityTypes);
        }

        public virtual IEnumerable<(Type Type, string Name)> GetKeyColumns(Type keyType) {
            return new DefaultKeyColumnExtractor().Extract(keyType);
        }

        public virtual string GetCollectionName(Type type) {
            return type.Name;
        }

        public ICollectionStorageSettings Configure(string collectionName, HashSet<Type> types) {
            return null;
        }
    }
}