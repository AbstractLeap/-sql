﻿namespace Leap.Data.Schema {
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;

    using Fasterflect;

    using Leap.Data.Utilities;

    internal class DefaultKeyTypeExtractor {
        public Type Extract(string tableName, IEnumerable<Type> entityTypes) {
            if (!entityTypes.Any()) {
                throw new ArgumentException($"You must pass at least one type to {nameof(this.Extract)}", nameof(entityTypes));
            }

            var indicatorType = entityTypes.First();
            var possibleMembers = indicatorType.Members(
                MemberTypes.Property | MemberTypes.Field,
                Flags.InstanceAnyVisibility | Flags.ExcludeBackingMembers,
                "Id",
                "Key",
                $"{tableName}Id",
                $"{tableName}Key");
            if (possibleMembers.Count != 1) {
                throw new Exception($"Unable to determine type of identifier for table {tableName} using indicator type {indicatorType}");
            }

            return possibleMembers[0].PropertyOrFieldType();
        }
    }
}