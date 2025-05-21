namespace TildeSql.Internal {
    using System;

    internal class TotalAccessor : ITotalAccessor, ITotalSetter {
        public long? total;

        public long Total => this.total ?? throw new Exception("Total not set. You must execute (and read at least one row of) the query before accessing the total");

        public void SetTotal(long total) {
            this.total = total;
        }
    }
}