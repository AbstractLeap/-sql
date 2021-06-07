namespace Leap.Data.Utilities {
    using System;

    using Leap.Data.Schema;

    public static class KeyPathHelper {
        public static string GetKeyPath(this Collection collection) {
            if (collection.KeyMembers.Length == 1 && collection.KeyMembers[0].PropertyOrFieldType().IsPrimitiveType()) {
                return collection.KeyMembers[0].Name;
            }

            throw new NotSupportedException();
        }
    }
}