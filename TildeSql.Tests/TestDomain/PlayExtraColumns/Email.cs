namespace TildeSql.Tests.TestDomain.PlayExtraColumns {
    class Email {
        private readonly string address;

        public Email(string emailAddress) {
            this.address = emailAddress;
        }

        public string Address => this.address;

        protected bool Equals(Email other) {
            return this.Address == other.Address;
        }

        public override bool Equals(object obj) {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return this.Equals((Email)obj);
        }

        public override int GetHashCode() {
            return (this.Address != null ? this.Address.GetHashCode() : 0);
        }
    }
}