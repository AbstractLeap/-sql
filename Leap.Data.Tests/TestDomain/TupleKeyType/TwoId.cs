namespace Leap.Data.Tests.TestDomain.TupleKeyType {
    using System;

    record TwoId {
        private readonly Guid id;

        public TwoId() {
            this.id = Guid.NewGuid();
        }

        public TwoId(Guid id) {
            this.id = id;
        }
    }
}