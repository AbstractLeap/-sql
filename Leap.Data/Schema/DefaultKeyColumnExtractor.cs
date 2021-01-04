namespace Leap.Data.Schema {
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using System.Runtime.CompilerServices;

    using Fasterflect;

    using Leap.Data.Utilities;

    internal class DefaultKeyColumnExtractor {
        public IEnumerable<Column> Extract(Type keyType) {
            return keyType.Members(MemberTypes.Property | MemberTypes.Field, Flags.InstanceAnyDeclaredOnly | Flags.ExcludeBackingMembers)
                          .Where(m => m.Name != "EqualityContract") // compiler generated for records
                          .Select(m => new Column(m.PropertyOrFieldType(), m.Name));
        }
    }
}