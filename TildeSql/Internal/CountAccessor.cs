namespace TildeSql.Internal {
    using System;

    internal class CountAccessor : ICountAccessor, ICountSetter {
        public long? total;

        public long Count => this.total ?? throw new Exception("Total not set. You must execute (and read at least one row of) the query before accessing the total");

        public void SetTotal(long total) {
            this.total = total;
        }
    }
}