namespace TildeSql.Tests.TestDomain.MultiTableType {
    class Person {
        protected bool Equals(Person other) {
            return Equals(this.id, other.id);
        }

        public override bool Equals(object obj) {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((Person)obj);
        }

        public override int GetHashCode() {
            return (this.id != null ? this.id.GetHashCode() : 0);
        }

        private readonly Name name;

        private readonly PersonId id;

        public Person(Name name) {
            this.name = name;
            this.id   = new PersonId();
        }

        public PersonId Id => this.id;

        public Name Name => this.name;
    }
}