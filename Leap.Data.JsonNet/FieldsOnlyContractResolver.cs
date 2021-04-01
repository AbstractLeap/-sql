namespace Leap.Data.JsonNet {
    using System;
    using System.Collections.Generic;
    using System.Reflection;
    using System.Runtime.Serialization;

    using Newtonsoft.Json;
    using Newtonsoft.Json.Serialization;

    public class FieldsOnlyContractResolver: DefaultContractResolver
    {
        protected override List<MemberInfo> GetSerializableMembers(Type objectType) {
            var members = new List<MemberInfo>();
            members.AddRange(objectType.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic));
            var baseType = objectType.BaseType;
            while (baseType != null && baseType != typeof(object)) {
                members.AddRange(baseType.GetFields(BindingFlags.Instance | BindingFlags.NonPublic)); // NOTE we don't get the public ones here as they're already returned above.
                baseType = baseType.BaseType;
            }

            return members;
        }

        protected override JsonProperty CreateProperty(MemberInfo member, MemberSerialization memberSerialization) {
            return base.CreateProperty(member, MemberSerialization.Fields);
        }

        protected override JsonObjectContract CreateObjectContract(Type objectType) {
            var objectContract = base.CreateObjectContract(objectType);
            objectContract.MemberSerialization = MemberSerialization.Fields;
            objectContract.DefaultCreator      = () => FormatterServices.GetUninitializedObject(ReflectionUtils.EnsureNotNullableType(objectType));
            return objectContract;
        }
    }

    public static class ReflectionUtils {
        public static bool IsNullableType(Type t) {
            return (t.IsGenericType && t.GetGenericTypeDefinition() == typeof(Nullable<>));
        }

        public static Type EnsureNotNullableType(Type t) {
            return (IsNullableType(t)) ? Nullable.GetUnderlyingType(t) : t;
        }
    }
}
