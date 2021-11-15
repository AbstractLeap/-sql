namespace Leap.Data.Tests.TestDomain.TupleKeyType {
    class TupleKeyTypeThing {
        private readonly OneId oneId;

        private readonly TwoId twoId;

        private readonly string name;

        public TupleKeyTypeThing(OneId one, TwoId two, string name) {
            this.oneId = one;
            this.twoId = two;
            this.name  = name;
        }

        public string Name => this.name;

        public OneId OneId => this.oneId;

        public TwoId TwoId => this.twoId;
    }
}