namespace Leap.Data.Tests.TestDomain.MultiFieldKeyType {
    using System;

    record MultiFieldId {
        private readonly Guid leftId;

        private readonly Guid rightId;

        public MultiFieldId() {
            this.leftId  = Guid.NewGuid();
            this.rightId = Guid.NewGuid();
        }

        public MultiFieldId(Guid leftId, Guid rightId) {
            this.leftId  = leftId;
            this.rightId = rightId;
        }

        public Guid LeftId => this.leftId;

        public Guid RightId => this.rightId;
    }
}