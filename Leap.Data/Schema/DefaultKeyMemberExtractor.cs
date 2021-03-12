namespace Leap.Data.Schema {
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;

    using Fasterflect;

    public class DefaultKeyMemberExtractor {
        public MemberInfo GetKeyMember(string collectionName, IEnumerable<Type> entityTypes) {
            if (!entityTypes.Any()) {
                throw new ArgumentException($"You must pass at least one type to {nameof(this.GetKeyMember)}", nameof(entityTypes));
            }

            var indicatorType = entityTypes.First();
            var instanceMembers = indicatorType.Members(MemberTypes.Field | MemberTypes.Property, Flags.InstanceAnyVisibility | Flags.ExcludeBackingMembers);
            var candidateIdNames = new[] { "id", "key", $"{indicatorType.Name}id", $"{indicatorType.Name}key", $"{collectionName}id", $"{collectionName}key" };
            var idNamedMembers = instanceMembers.Where(m => candidateIdNames.Contains(m.Name, StringComparer.InvariantCultureIgnoreCase)).ToArray();
            if (idNamedMembers.Length == 1) {
                return idNamedMembers[0];
            }

            if (idNamedMembers.Length > 1) {
                var grouped = idNamedMembers.GroupBy(m => m.Name.ToLowerInvariant());
                if (grouped.Count() != 1) {
                    throw new Exception($"Unable to determine type of identifier for collection {collectionName} using indicator type {indicatorType}");
                }

                return grouped.First().OrderByDescending(m => m.MemberType == MemberTypes.Field).First();
            }

            // no id named members, let's look for anything that ends in id
            var endingInIdNamedMembers = instanceMembers.Where(m => m.Name.EndsWith("id", StringComparison.InvariantCultureIgnoreCase)).ToArray();
            if (endingInIdNamedMembers.Length == 1) {
                return endingInIdNamedMembers[0];
            }

            if (endingInIdNamedMembers.Length > 1) {
                var grouped = endingInIdNamedMembers.GroupBy(m => m.Name.ToLowerInvariant());
                if (grouped.Count() != 1) {
                    throw new Exception($"Unable to determine type of identifier for collection {collectionName} using indicator type {indicatorType}");
                }

                return grouped.First().OrderByDescending(m => m.MemberType == MemberTypes.Field).First();
            }

            throw new Exception($"Unable to determine the field or property that contains the id for collection {collectionName}");
        }
    }
}