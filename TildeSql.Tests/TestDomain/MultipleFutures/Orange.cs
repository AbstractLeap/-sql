namespace TildeSql.Tests.TestDomain.MultipleFutures {
    using System;

    class Orange {
        public Guid Id { get; set; } = Guid.NewGuid();

        public string Type { get; set; }
    }
}