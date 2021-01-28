namespace Leap.Data.Schema {
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;

    using Fasterflect;

    using Leap.Data.Utilities;

    internal class DefaultKeyColumnExtractor {
        public IEnumerable<(Type, string)> Extract(Type keyType) {
            var members = keyType.Members(MemberTypes.Property | MemberTypes.Field, Flags.InstanceAnyDeclaredOnly | Flags.ExcludeBackingMembers)
                          .Where(m => m.Name != "EqualityContract") // compiler generated for records
                          .Select(m => (m.PropertyOrFieldType(), m.Name))
                          .ToArray();
            if (members.Length == 2 && members.Select(m => m.Name.ToUpperInvariant()).Distinct().Count() == 1) {
                return members.Take(1);
            }

            return members;
        }
    }
}