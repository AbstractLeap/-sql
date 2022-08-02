namespace TildeSql.Schema.KeyFactories {
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using Fasterflect;

    using TildeSql.Utilities;

    class Node : INode {
        private readonly List<INode> nodes = new();

        private readonly Type type;

        public Node(Type type) {
            this.type = type;
        }

        public object GetValue(object[] row) {
            var vals = this.nodes.Select(n => n.GetValue(row)).ToArray();
            return this.type.IsPrimitiveType() && vals.Length == 1 ? vals[0] : this.type.CreateInstance(vals);
        }

        public void Add(INode node) {
            this.nodes.Add(node);
        }
    }
}