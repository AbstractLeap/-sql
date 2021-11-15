namespace Leap.Data.Tests.TestDomain.TypedSerialization {
    class Foo : IFoo {
        private readonly bool isIt;

        public Foo(bool isIt) {
            this.isIt = isIt;
        }

        public bool IsIt => this.isIt;
    }
}