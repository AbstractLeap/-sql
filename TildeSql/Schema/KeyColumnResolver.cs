namespace TildeSql.Schema {
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;

    using Fasterflect;

    using TildeSql.Schema.Columns;
    using TildeSql.Utilities;

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
                foreach (var keyColumn in ResolveKeyColumns(
                             keyMemberInfo,
                             entry.Index,
                             keyMemberInfo,
                             new List<MemberInfo>(),
                             1,
                             this.keyMembers.Length > 1 ? 0 : null,
                             this.keyMembers.Length == 1)) {
                    yield return keyColumn;
                }
            }
        }

        IEnumerable<(KeyColumn, IKeyColumnValueAccessor)> ResolveKeyColumns(
            MemberInfo keyMemberInfo,
            int keyMemberIdx,
            MemberInfo memberInfo,
            ICollection<MemberInfo> memberAccessors,
            int currentDepth,
            int? splitDepth,
            bool single) {
            var memberType = memberInfo.PropertyOrFieldType();
            if (memberType.IsPrimitiveType()) {
                var name = string.Join("_", memberAccessors.Skip(splitDepth ?? 0).Union(single ? Enumerable.Empty<MemberInfo>() : new[] { memberInfo }).Select(m => m.Name));
                var resultantMemberAccessors = new List<MemberInfo>();
                if (this.keyMembers.Length > 1) {
                    // the first accessor is from a tuple matching the keytype
                    resultantMemberAccessors.Add(this.keyType.GetFields()[keyMemberIdx]);
                }

                if (!keyMemberInfo.PropertyOrFieldType().IsPrimitiveType()) {
                    resultantMemberAccessors.AddRange(memberAccessors.Skip(1).Union(new[] { memberInfo }));
                }

                yield return (new KeyColumn(memberType, name, this.collection, keyMemberInfo, resultantMemberAccessors.ToArray()),
                                 new NestedKeyColumnValueAccessor(resultantMemberAccessors.ToArray()));
            }
            else {
                var members = GetKeyMemberInfos(memberType);
                foreach (var member in members) {
                    foreach (var keyColumn in ResolveKeyColumns(
                                 keyMemberInfo,
                                 keyMemberIdx,
                                 member,
                                 memberAccessors.Union(new[] { memberInfo }).ToList(),
                                 currentDepth + 1,
                                 splitDepth.HasValue ? splitDepth : (members.Length > 1 ? currentDepth : null),
                                 members.Length == 1)) {
                        yield return keyColumn;
                    }
                }
            }
        }

        MemberInfo[] GetKeyMemberInfos(Type keyType) {
            var members = keyType.Members(MemberTypes.Property | MemberTypes.Field, Flags.InstanceAnyVisibility | Flags.ExcludeBackingMembers)
                                 .Where(m => m.Name != "EqualityContract") // compiler generated for records
                                 .ToArray();

            var fields = members.Where(m => m.MemberType == MemberTypes.Field).ToArray();
            var properties = members.Where(m => m.MemberType == MemberTypes.Property).ToArray();

            return fields.Any() ? fields : properties;
        }
    }
}