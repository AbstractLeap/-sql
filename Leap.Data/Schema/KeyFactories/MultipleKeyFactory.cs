namespace Leap.Data.Schema.KeyFactories {
    using System;
    using System.Linq;

    using Leap.Data.Schema.Columns;
    using Leap.Data.Utilities;

    class MultipleKeyFactory : IKeyFactory {
        private readonly Node rootNode;

        public MultipleKeyFactory(KeyColumn[] keyColumns, Type keyType) {
            var valuePaths = keyColumns.Select(k => (k.MemberAccessors, k.Collection.GetColumnIndex(k.Name))).ToArray();
            var i = 0;
            this.rootNode = new Node(keyType);
            var lookup = valuePaths.ToDictionary(i => i.Item2, i => this.rootNode);
            var maxMemberAccessorsLength = valuePaths.Max(p => p.MemberAccessors.Length);
            while (i < maxMemberAccessorsLength) {
                foreach (var path in valuePaths) {
                    if (i <= path.MemberAccessors.Length - 1) {
                        var parent = lookup[path.Item2];
                        if (i == path.MemberAccessors.Length - 1) {
                            parent.Add(new Leaf(path.Item2));
                        }
                        else if (i < path.MemberAccessors.Length - 1) {
                            var node = new Node(path.MemberAccessors[i].PropertyOrFieldType());
                            parent.Add(node);
                            lookup[path.Item2] = node;
                        }
                    }
                }

                i++;
            }
        }

        public object Create(object[] row) {
            return this.rootNode.GetValue(row);
        }
    }
}