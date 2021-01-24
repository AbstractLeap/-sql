namespace Leap.Data.Queries {
    public class EntityQuery<TEntity> : QueryBase<TEntity>
        where TEntity : class {
        public string WhereClause { get; set; }

        public string OrderByClause { get; set; }

        public int? Limit { get; set; }

        public int? Offset { get; set; }

        public override void Accept(IQueryVisitor visitor)
        {
            visitor.VisitEntityQuery(this);
        }
    }
}