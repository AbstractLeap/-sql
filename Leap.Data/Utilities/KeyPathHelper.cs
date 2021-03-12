namespace Leap.Data.Utilities {
    using System;

    using Leap.Data.Schema;

    public static class KeyPathHelper {
        public static string GetKeyPath(this Collection collection) {
            if (collection.KeyMember.PropertyOrFieldType().IsPrimitiveKeyType()) {
                return collection.KeyMember.Name;
            }

            throw new NotSupportedException();
        }
    }
}