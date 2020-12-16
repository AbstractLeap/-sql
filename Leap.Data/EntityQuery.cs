namespace Leap.Data {
    public class EntityQuery<T> {
        public string WhereClause { get; set; }

        public string OrderByClause { get; set; }

        public int Limit { get; set; }

        public int Offset { get; set; }
    }
}