namespace TildeSql.Tests.TestDomain.TupleKeyType {
    using System;

    record OneId {
        private readonly Guid id;

        public OneId() {
            this.id = Guid.NewGuid();
        }

        public OneId(Guid id) {
            this.id = id;
        }
    }
}