namespace Leap.Data.Schema.KeyFactories {
    class Leaf : INode {
        private readonly int idx;

        public Leaf(int idx) {
            this.idx = idx;
        }

        public object GetValue(object[] row) {
            return row[this.idx];
        }
    }
}