namespace Leap.Data.Queries {
    using System.Threading;
    using System.Threading.Tasks;

    using Leap.Data.Schema;

    public class KeyQuery<TEntity, TKey> : QueryBase<TEntity>
        where TEntity : class {
        public KeyQuery(TKey key, Table table) : base(table) {
            this.Key = key;
        }

        public TKey Key { get; }

        public override void Accept(IQueryVisitor visitor) {
            visitor.VisitKeyQuery(this);
        }

        public override ValueTask AcceptAsync(IAsyncQueryVisitor visitor, CancellationToken cancellationToken = default) {
            return visitor.VisitKeyQueryAsync(this, cancellationToken);
        }
    }
}