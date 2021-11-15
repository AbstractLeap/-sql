﻿namespace Leap.Data.Tests.TestDomain.TypedSerialization {
    using System;

    record PersonId {
        private readonly Guid id;

        public PersonId(Guid id) {
            this.id = id;
        }

        public PersonId() {
            this.id = Guid.NewGuid();
        }

        public Guid Id => this.id;
    }
}