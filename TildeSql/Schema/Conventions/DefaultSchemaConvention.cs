namespace TildeSql.Schema.Conventions {
    using System;
    using System.Collections.Generic;
    using System.Reflection;

    public class DefaultSchemaConvention : IStorageSchemaConvention,
                                           IOptimisticConcurrencySchemaConvention,
                                           ICollectionNamingSchemaConvention,
                                           IKeyMemberSchemaConvention,
                                           IKeyComputedSchemaConvention {
        public virtual MemberInfo[] GetKeyMember(string collectionName, IEnumerable<Type> entityTypes) {
            return new DefaultKeyMemberExtractor().GetKeyMember(collectionName, entityTypes);
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