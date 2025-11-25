namespace TildeSql.Tests.TestDomain.Join {
    using System;

    public record JoinId {
        public JoinId() {
            this.Id = Guid.NewGuid();
        }

        public JoinId(Guid id) {
            this.Id = id;
        }

        public Guid Id { get; init; }
    }
}