namespace Leap.Data.Schema {
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;

    using Fasterflect;

    using Leap.Data.Utilities;

    internal class DefaultKeyTypeMemberExtractor {
        public IEnumerable<MemberInfo> Extract(Type keyType) {
            if (keyType.IsPrimitiveKeyType()) {
                throw new ArgumentOutOfRangeException(nameof(keyType), "Key type must be complex");
            }

            var members = keyType.Members(MemberTypes.Property | MemberTypes.Field, Flags.InstanceAnyDeclaredOnly | Flags.ExcludeBackingMembers)
                                 .Where(m => m.Name != "EqualityContract") // compiler generated for records
                                 .ToArray();
            if (members.Length == 2 && members.Select(m => m.Name.ToUpperInvariant()).Distinct().Count() == 1) {
                return members.Take(1);
            }

            return members;
        }
    }
}