namespace TildeSql.Internal {
    using System;
    using System.Linq;

    internal static class TypeSerializer {
        public static string Serialize(this Type type) {
            if (!type.IsGenericType) {
                return type.Name;
            }

            var genericArguments = type.GetGenericArguments();
            return $"{type.Name}[{string.Join(", ", genericArguments.Select(t => t.AssemblyQualifiedName))}]";
        }
    }
}