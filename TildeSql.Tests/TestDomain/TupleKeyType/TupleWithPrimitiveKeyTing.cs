namespace TildeSql.Tests.TestDomain.TupleKeyType {
    using System;

    class TupleWithPrimitiveKeyTing {
        private readonly OneId oneId;

        private readonly long atTime;

        private readonly string emailAddress;

        public TupleWithPrimitiveKeyTing(OneId one, DateTime time, string email) {
            this.oneId        = one;
            this.atTime       = time.Ticks;
            this.emailAddress = email;
        }

        public string Email => this.emailAddress;

        public DateTime AtTime => new DateTime(this.atTime);

        public OneId OneId => this.oneId;
    }
}