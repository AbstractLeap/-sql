namespace Leap.Data.Queries {
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;

    using Leap.Data.Schema;

    public class EntityQuery<TEntity> : QueryBase<TEntity>
        where TEntity : class {
        public EntityQuery(Collection collection)
            : base(collection) { }

        public string WhereClause { get; set; }

        public string OrderByClause { get; set; }

        public int? Limit { get; set; }

        public int? Offset { get; set; }

        public IDictionary<string, object> WhereClauseParameters { get; set; }

        public override void Accept(IQueryVisitor visitor) {
            visitor.VisitEntityQuery(this);
        }

        public override ValueTask AcceptAsync(IAsyncQueryVisitor visitor, CancellationToken cancellationToken = default) {
            return visitor.VisitEntityQueryAsync(this, cancellationToken);
        }
    }
}