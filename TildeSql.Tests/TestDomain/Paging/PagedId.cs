namespace TildeSql.Tests.TestDomain.Paging {
    using System;

    public record PagedId {
        public PagedId() {
            this.Id = Guid.NewGuid();
        }

        public PagedId(Guid id) {
            this.Id = id;
        }

        public Guid Id { get; init; }
    }
}