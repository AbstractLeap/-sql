﻿namespace Leap.Data.IdentityMap {
    using System;

    using Fasterflect;

    static class IdentityMapExtensions {
        public static bool TryGetValue<TEntity>(this IdentityMap identityMap, Type keyType, object key, out Document<TEntity> entity)
            where TEntity : class {
            var entityType = typeof(TEntity);
            var arguments = new[] { key, null };
            var result = (bool)identityMap.CallMethod(
                new[] { entityType, keyType },
                nameof(IdentityMap.TryGetValue),
                new[] { keyType, typeof(Document<>).MakeGenericType(entityType).MakeByRefType() },
                Flags.ExactBinding | Flags.InstancePublicDeclaredOnly,
                arguments);
            entity = result ? (Document<TEntity>)arguments[1] : null;
            return result;
        }

        public static void Add<TEntity>(this IdentityMap identityMap, Type keyType, object key, Document<TEntity> entity) {
            identityMap.CallMethod(new[] { typeof(TEntity), keyType }, nameof(IdentityMap.Add), key, entity);
        }

        public static void Remove<TEntity>(this IdentityMap identityMap, Type keyType, object key) {
            identityMap.CallMethod(new[] { typeof(TEntity), keyType }, nameof(IdentityMap.Remove), key);
        }
    }
}