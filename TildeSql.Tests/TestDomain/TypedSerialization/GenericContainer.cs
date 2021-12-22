namespace TildeSql.Tests.TestDomain.TypedSerialization {
    class GenericContainer {
        private readonly Name name;

        private readonly PersonId id;

        public GenericContainer(Name name) {
            this.name = name;
            this.id   = new PersonId();
        }

        public PersonId Id => this.id;

        public Name Name => this.name;

        public IFoo MyFoo { get; set; }
    }
}