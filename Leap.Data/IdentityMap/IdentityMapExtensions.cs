namespace Leap.Data.IdentityMap {
    using System;

    using Fasterflect;

    static class IdentityMapExtensions {
        public static bool TryGetValue<TEntity>(this IdentityMap identityMap, Type keyType, object key, out TEntity entity)
            where TEntity : class {
            var arguments = new[] { key, null };
            var result = (bool)identityMap.CallMethod(
                new[] { typeof(TEntity), keyType },
                nameof(IdentityMap.TryGetValue),
                new[] { keyType, typeof(TEntity).MakeByRefType() },
                Flags.ExactBinding | Flags.InstancePublicDeclaredOnly,
                arguments);
            entity = result ? (TEntity)arguments[1] : null;
            return result;
        }

        public static void Add<TEntity>(this IdentityMap identityMap, Type keyType, object key, TEntity entity) {
            identityMap.CallMethod(new[] { typeof(TEntity), keyType }, nameof(IdentityMap.Add), key, entity);
        }

        public static void Remove<TEntity>(this IdentityMap identityMap, Type keyType, object key) {
            identityMap.CallMethod(new[] { typeof(TEntity), keyType }, nameof(IdentityMap.Remove), key);
        }
    }
}