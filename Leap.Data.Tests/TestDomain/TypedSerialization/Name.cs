namespace Leap.Data.Tests.TestDomain.TypedSerialization {
    record Name {
        private readonly string surname;

        private readonly string givenNames;

        public Name(string givenNames, string surname) {
            this.givenNames = givenNames;
            this.surname    = surname;
        }

        public string Surname => this.surname;

        public string GivenNames => this.givenNames;
    }
}