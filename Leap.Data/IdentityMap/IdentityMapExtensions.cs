namespace Leap.Data.IdentityMap {
    using System;

    using Fasterflect;

    static class IdentityMapExtensions {
        // TODO Fix
        public static bool TryGetValue<TEntity>(this IdentityMap identityMap, Type keyType, object key, out TEntity entity)
            where TEntity : class {
            var entityType = typeof(TEntity);
            var arguments = new[] { key, null };
            var result = (bool)identityMap.CallMethod(
                new[] { entityType, keyType },
                nameof(IdentityMap.TryGetValue),
                new[] { keyType, entityType.MakeByRefType() },
                Flags.ExactBinding | Flags.InstancePublicDeclaredOnly,
                arguments);
            entity = result ? (TEntity)arguments[1] : null;
            return result;
        }

        public static void Add<TEntity>(this IdentityMap identityMap, Type keyType, object key, TEntity entity) {
            identityMap.CallMethod(new[] { typeof(TEntity), keyType }, nameof(IdentityMap.Add), key, entity);
        }
    }
}