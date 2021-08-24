namespace Leap.Data.Schema {
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;

    using Fasterflect;

    using Leap.Data.Schema.Columns;
    using Leap.Data.Utilities;

    class KeyColumnResolver {
        private readonly Type keyType;

        private readonly MemberInfo[] keyMembers;

        private readonly Collection collection;

        public KeyColumnResolver(Type keyType, MemberInfo[] keyMembers, Collection collection) {
            this.keyType    = keyType;
            this.keyMembers = keyMembers;
            this.collection = collection;
        }

        public IEnumerable<(KeyColumn, IKeyColumnValueAccessor)> ResolveKeyColumns(bool isKeyComputed = false) {
            if (this.keyType.IsPrimitiveType()) {
                if (this.keyMembers.Length > 1) {
                    throw new NotSupportedException();
                }

                yield return (new KeyColumn(this.keyType, this.keyMembers[0].Name, this.collection, this.keyMembers[0], Array.Empty<MemberInfo>()) { IsComputed = isKeyComputed },
                                 new PrimitiveKeyColumnValueAccessor());
                yield break;
            }

            foreach (var entry in this.keyMembers.AsSmartEnumerable()) {
                var keyMemberInfo = entry.Value;
                foreach (var keyColumn in ResolveKeyColumns(keyMemberInfo, keyMemberInfo, new List<MemberInfo>(), this.keyMembers.Length == 1)) {
                    yield return keyColumn;
                }
            }
        }

        IEnumerable<(KeyColumn, IKeyColumnValueAccessor)> ResolveKeyColumns(MemberInfo keyMemberInfo, MemberInfo memberInfo, ICollection<MemberInfo> memberAccessors, bool single) {
            var memberType = memberInfo.PropertyOrFieldType();
            if (memberType.IsPrimitiveType()) {
                var name = single
                               ? string.Join("_", memberAccessors.Skip(memberAccessors.Count > 1 ? 1 : 0).Select(m => m.Name))
                               : string.Join("_", memberAccessors.Skip(this.keyMembers.Length == 1 ? 1 : 0).Union(new[] { memberInfo }).Select(m => m.Name));

                //var name = this.keyMembers.Length == 1
                //               ? (single
                //                      ? string.Join("_", memberAccessors.Skip(1).Union(new[] { memberInfo }).Select(m => m.Name))
                //                      : string.Join("_", memberAccessors.Select(m => m.Name)))
                //               : string.Join("_", memberAccessors.Union(new[] { memberInfo }).Select(m => m.Name));
                var resultantMemberAccessors = memberAccessors.Skip(1).Union(new[] { memberInfo }).ToArray();
                yield return (new KeyColumn(memberType, name, this.collection, keyMemberInfo, resultantMemberAccessors),
                                 new NestedKeyColumnValueAccessor(resultantMemberAccessors));
            }
            else {
                var members = GetKeyMemberInfos(memberType);
                foreach (var member in members) {
                    foreach (var keyColumn in ResolveKeyColumns(keyMemberInfo, member, memberAccessors.Union(new[] { memberInfo }).ToList(), members.Length == 1)) {
                        yield return keyColumn;
                    }
                }
            }
        }

        MemberInfo[] GetKeyMemberInfos(Type keyType) {
            var members = keyType.Members(MemberTypes.Property | MemberTypes.Field, Flags.InstanceAnyDeclaredOnly | Flags.ExcludeBackingMembers)
                                 .Where(m => m.Name != "EqualityContract") // compiler generated for records
                                 .ToArray();

            var fields = members.Where(m => m.MemberType == MemberTypes.Field).ToArray();
            var properties = members.Where(m => m.MemberType == MemberTypes.Property).ToArray();

            if (fields.Length == properties.Length && fields.All(fi => properties.Any(pi => string.Equals(fi.Name, pi.Name, StringComparison.OrdinalIgnoreCase)))) {
                members = fields;
            }

            return members;
        }
    }
}