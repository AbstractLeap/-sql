namespace Leap.Data.Schema.KeyFactories {
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using Fasterflect;

    using Leap.Data.Schema.Columns;
    using Leap.Data.Utilities;

    class TupleKeyFactory : IKeyFactory {
        private readonly Type keyType;

        private readonly List<IKeyFactory> keyFactories = new();

        public TupleKeyFactory(KeyColumn[] keyColumns, Type keyType) {
            this.keyType = keyType;
            var columnGroups = keyColumns.GroupBy(k => k.KeyMemberInfo);
            foreach (var columnGroup in columnGroups) {
                this.keyFactories.Add(new MultipleKeyFactory(columnGroup.ToArray(), columnGroup.Key.PropertyOrFieldType()));
            }
        }

        public object Create(object[] row) {
            var vals = this.keyFactories.Select(f => f.Create(row)).ToArray();
            return this.keyType.CreateInstance(vals);
        }
    }
}