namespace TildeSql.Tests.TestDomain.PlayExtraColumns {
    class Person {
        private readonly Name name;

        private readonly PersonId id;

        private Email email;

        public Person(Name name) {
            this.name = name;
            this.id   = new PersonId();
        }

        public PersonId Id => this.id;

        public Name Name => this.name;

        public Email Email {
            get => this.email;
            set => this.email = value;
        }
    }
}