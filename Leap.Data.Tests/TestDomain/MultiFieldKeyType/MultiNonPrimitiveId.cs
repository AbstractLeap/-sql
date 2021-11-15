namespace Leap.Data.Tests.TestDomain.MultiFieldKeyType {
    record MultiNonPrimitiveId {
        private readonly InsideId leftId;

        private readonly InsideId rightId;

        public MultiNonPrimitiveId() {
            this.leftId  = new InsideId();
            this.rightId = new InsideId();
        }

        public MultiNonPrimitiveId(InsideId leftId, InsideId rightId) {
            this.leftId  = leftId;
            this.rightId = rightId;
        }

        public InsideId LeftId => this.leftId;

        public InsideId RightId => this.rightId;
    }
}