﻿namespace Leap.Data.Internal {
    using System;
    using System.Linq;
    using System.Reflection;

    using Fasterflect;

    using Leap.Data.Schema;

    class DefaultTypedKeyExtractor<TEntity, TKey> {
        // ReSharper disable once StaticMemberInGenericType
        private static readonly MemberInfo MemberInfo;

        static DefaultTypedKeyExtractor() {
            var keyType = typeof(TKey);
            var candidateIdMembers = typeof(TEntity).Members(MemberTypes.Field | MemberTypes.Property, Flags.AllMembers | Flags.ExcludeBackingMembers)
                                                    .Where(
                                                        m => (m is FieldInfo fieldInfo && fieldInfo.FieldType == keyType)
                                                             || (m is PropertyInfo propertyInfo && propertyInfo.PropertyType == keyType))
                                                    .ToArray();
            // support properties with a backing field
            if (candidateIdMembers.Length == 2 && candidateIdMembers.Select(m => m.Name.ToUpperInvariant()).Distinct().Count() == 1) {
                MemberInfo = candidateIdMembers.OrderByDescending(m => m is FieldInfo).First();
                return;
            }
            
            if (candidateIdMembers.Length != 1) {
                throw new Exception(
                    $"Unable to determine key property or field on type {typeof(TEntity)} while extracting key values. Please override {nameof(Collection.KeyExtractor)} to provide custom extraction method");
            }

            MemberInfo = candidateIdMembers[0];
        }

        public TKey Extract(TEntity entity) {
            return (TKey)MemberInfo.Get(entity);
        }
    }
}