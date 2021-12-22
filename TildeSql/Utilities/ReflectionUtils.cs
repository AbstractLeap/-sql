namespace TildeSql.Utilities {
    using System;
    using System.Linq;
    using System.Reflection;

    using Fasterflect;

    public static class ReflectionUtils {
        public static object GetMemberValue<T>(MemberInfo memberInfo, T obj) {
            if (memberInfo.DeclaringType == typeof(T)) {
                return memberInfo.Get(obj);
            }

            if (memberInfo.DeclaringType.ContainsGenericParameters) {
                var resolvedMemberInfo = typeof(T).GetMember(memberInfo.Name, BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public).Single();
                return GetMemberValue(resolvedMemberInfo, obj);
            }

            if (memberInfo is FieldInfo fieldInfo) {
                return fieldInfo.GetValue(obj);
            }

            if (memberInfo is PropertyInfo propertyInfo) {
                return propertyInfo.GetValue(obj);
            }

            throw new NotSupportedException();
        }

        public static void SetMemberValue(MemberInfo memberInfo, object obj, object value) {
            if (memberInfo.DeclaringType == obj.GetType()) {
                memberInfo.Set(obj, value);
                return;
            }

            if (memberInfo.DeclaringType.ContainsGenericParameters) {
                var resolvedMemberInfo = obj.GetType().GetMember(memberInfo.Name, BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public).Single();
                SetMemberValue(resolvedMemberInfo, obj, value);
                return;
            }

            if (memberInfo is FieldInfo fieldInfo) {
                fieldInfo.SetValue(obj, value);
                return;
            }

            if (memberInfo is PropertyInfo propertyInfo) {
                propertyInfo.SetValue(obj, value);
                return;
            }

            throw new NotSupportedException();
        }
    }
}