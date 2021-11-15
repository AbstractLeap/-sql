namespace Leap.Data.Tests.TestDomain.Identity {
    class EntityWithIdentity {
        private long id;

        private string name;

        public long Id {
            get => this.id;
            set => this.id = value;
        }

        public string Name {
            get => this.name;
            set => this.name = value;
        }
    }
}