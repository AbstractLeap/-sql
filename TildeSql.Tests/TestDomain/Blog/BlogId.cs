namespace TildeSql.Tests.TestDomain.Blog {
    using System;

    public record BlogId {
        public BlogId() {
            this.Id = Guid.NewGuid();
        }

        public BlogId(Guid id) {
            this.Id = id;
        }

        public Guid Id { get; init; }
    }
}