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
            // See Remarks at https://docs.microsoft.com/en-us/dotnet/api/system.type.getfields?view=net-5.0
            var members = new HashSet<MemberInfo>();
            members.UnionWith(objectType.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic));
            var baseType = objectType.BaseType;
            while (baseType != null && baseType != typeof(object)) {
                // NOTE we don't get the public ones here as they're already returned above.
                // but we do get protected ones here as well, but they're already returned above as well so we have to filter them out
                members.UnionWith(baseType.GetFields(BindingFlags.Instance | BindingFlags.NonPublic).Where(f => !f.IsFamily));
                baseType = baseType.BaseType;
            }

            return members.ToList();
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
