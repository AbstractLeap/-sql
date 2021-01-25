namespace Leap.Data.Queries {
    using System.Threading;
    using System.Threading.Tasks;

    public class MultipleKeyQuery<TEntity, TKey> : QueryBase<TEntity>
        where TEntity : class {
        public MultipleKeyQuery(TKey[] keys) {
            this.Keys = keys;
        }

        public TKey[] Keys { get; }

        public override void Accept(IQueryVisitor visitor) {
            visitor.VisitMultipleKeyQuery(this);
        }

        public override ValueTask AcceptAsync(IAsyncQueryVisitor visitor, CancellationToken cancellationToken = default) {
            return visitor.VisitMultipleKeyQueryAsync(this, cancellationToken);
        }
    }
}