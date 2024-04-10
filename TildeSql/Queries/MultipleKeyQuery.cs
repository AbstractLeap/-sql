namespace TildeSql.Queries {
    using System.Threading;
    using System.Threading.Tasks;

    using TildeSql.Schema;

    public class MultipleKeyQuery<TEntity, TKey> : QueryBase<TEntity>, IMultipleKeyQuery
        where TEntity : class {
        public MultipleKeyQuery(TKey[] keys, Collection collection) : base(collection) {
            this.Keys = keys;
        }

        public TKey[] Keys { get; }

        public override void Accept(IQueryVisitor visitor) {
            visitor.VisitMultipleKeyQuery(this);
        }

        public override ValueTask AcceptAsync(IAsyncQueryVisitor visitor, CancellationToken cancellationToken = default) {
            return visitor.VisitMultipleKeyQueryAsync(this, cancellationToken);
        }

        public object[] ExpectedKeys() {
            return [..this.Keys];
        }
    }
}