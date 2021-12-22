namespace TildeSql.Tests.TestDomain.MultiFieldKeyType {
    using System;

    record InsideId {
        private readonly Guid id;

        public InsideId() {
            this.id = Guid.NewGuid();
        }

        public InsideId(Guid id) {
            this.id = id;
        }

        public Guid Id => this.id;
    }
}