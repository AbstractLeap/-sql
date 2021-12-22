namespace TildeSql.Utilities {
    using System;

    using TildeSql.Schema;

    public static class KeyPathHelper {
        public static string GetKeyPath(this Collection collection) {
            if (collection.KeyMembers.Length == 1 && collection.KeyMembers[0].PropertyOrFieldType().IsPrimitiveType()) {
                return collection.KeyMembers[0].Name;
            }

            throw new NotSupportedException();
        }
    }
}