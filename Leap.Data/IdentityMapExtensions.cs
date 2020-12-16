namespace Leap.Data {
    using System;
    using System.Collections.Concurrent;

    using Fasterflect;

    static class IdentityMapExtensions {
        private static ConcurrentDictionary<(Type entityType, Type keyType), MethodInvoker> identityInvokers = new ConcurrentDictionary<(Type entityType, Type keyType), MethodInvoker>();
        
        public static bool TryGetValue<TEntity>(this IdentityMap identityMap, Type keyType, object key, out TEntity entity)
            where TEntity : class {
            var arguments = new object[] { key, null };
            var result = (bool)identityMap.CallMethod(
                new[] { typeof(TEntity), keyType },
                nameof(IdentityMap.TryGetValue),
                new[] { keyType, typeof(TEntity).MakeByRefType() },
                Flags.ExactBinding | Flags.InstancePublicDeclaredOnly,
                arguments);
            entity = result ? (TEntity)arguments[1] : null;
            return result;
        }
    }
}