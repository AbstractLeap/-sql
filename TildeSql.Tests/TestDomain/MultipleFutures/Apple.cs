namespace TildeSql.Tests.TestDomain.MultipleFutures {
    using System;

    class Apple {
        public Guid Id { get; set; } = Guid.NewGuid();

        public string Type { get; set; }
    }
}