namespace Leap.Data.Utilities {
    using System;
    using System.Reflection;

    public static class TypeHelpers {
        public static Type PropertyOrFieldType(this MemberInfo memberInfo) {
            if (memberInfo is FieldInfo fieldInfo) {
                return fieldInfo.FieldType;
            }

            if (memberInfo is PropertyInfo propertyInfo) {
                return propertyInfo.PropertyType;
            }

            throw new Exception($"Expected FieldInfo or PropertyInfo but got {memberInfo}");
        }
    }
}