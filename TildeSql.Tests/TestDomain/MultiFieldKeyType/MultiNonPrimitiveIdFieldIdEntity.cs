namespace TildeSql.Tests.TestDomain.MultiFieldKeyType {
    class MultiNonPrimitiveIdFieldIdEntity {
        private readonly MultiNonPrimitiveId id;

        private readonly string name;

        public MultiNonPrimitiveIdFieldIdEntity(string name) {
            this.id   = new MultiNonPrimitiveId();
            this.name = name;
        }

        public string Name => this.name;

        public MultiNonPrimitiveId Id => this.id;
    }
}