namespace Leap.Data.Schema.KeyFactories {
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using Fasterflect;

    class Node : INode {
        private readonly List<INode> nodes = new();

        private readonly Type type;

        public Node(Type type) {
            this.type = type;
        }

        public object GetValue(object[] row) {
            var vals = this.nodes.Select(n => n.GetValue(row)).ToArray();
            return this.type.CreateInstance(vals);
        }

        public void Add(INode node) {
            this.nodes.Add(node);
        }
    }
}