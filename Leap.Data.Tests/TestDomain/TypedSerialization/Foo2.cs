namespace Leap.Data.Tests.TestDomain.TypedSerialization {
    class Foo2 : IFoo {
        private bool isIt;

        public bool IsIt {
            get => this.isIt;
            set => this.isIt = value;
        }
    }
}