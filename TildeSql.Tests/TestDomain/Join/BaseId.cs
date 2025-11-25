namespace TildeSql.Tests.TestDomain.Join {
    using System;

    public record BaseId {
        public BaseId() {
            this.Id = Guid.NewGuid();
        }

        public BaseId(Guid id) {
            this.Id = id;
        }

        public Guid Id { get; init; }
    }
}