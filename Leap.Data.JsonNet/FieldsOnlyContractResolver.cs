namespace Leap.Data.JsonNet {
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using System.Runtime.Serialization;

    using Newtonsoft.Json;
    using Newtonsoft.Json.Serialization;

    public class FieldsOnlyContractResolver: DefaultContractResolver
    {
        protected override List<MemberInfo> GetSerializableMembers(Type objectType) {
            return objectType.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic).Cast<MemberInfo>().ToList();
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
