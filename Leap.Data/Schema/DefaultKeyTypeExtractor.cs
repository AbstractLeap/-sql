namespace Leap.Data.Schema {
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;

    using Fasterflect;

    using Leap.Data.Utilities;

    public class DefaultKeyTypeExtractor {
        public Type Extract(string collectionName, IEnumerable<Type> entityTypes) {
            if (!entityTypes.Any()) {
                throw new ArgumentException($"You must pass at least one type to {nameof(this.Extract)}", nameof(entityTypes));
            }

            var indicatorType = entityTypes.First();
            var possibleMembers = indicatorType.Members(
                MemberTypes.Property | MemberTypes.Field,
                Flags.InstanceAnyVisibility | Flags.ExcludeBackingMembers,
                "id",
                "Id",
                "key",
                "Key",
                $"{indicatorType.Name}Id",
                $"{indicatorType.Name}Key",
                $"{collectionName}Id",
                $"{collectionName}Key");

            if (possibleMembers.Count != 1) {
                var grouped = possibleMembers.GroupBy(m => m.Name.ToLowerInvariant());
                if (grouped.Count() != 1) {
                    throw new Exception($"Unable to determine type of identifier for collection {collectionName} using indicator type {indicatorType}");
                }

                return grouped.First().OrderByDescending(m => m.MemberType == MemberTypes.Field).First().PropertyOrFieldType();
            }

            return possibleMembers[0].PropertyOrFieldType();
        }
    }
}