namespace Leap.Data.Tests.TestDomain.MultiFieldKeyType {
    class MultiFieldIdEntity {
        private readonly MultiFieldId id;

        private readonly string name;

        public MultiFieldIdEntity(string name) {
            this.id   = new MultiFieldId();
            this.name = name;
        }

        public string Name => this.name;

        public MultiFieldId Id => this.id;
    }
}