namespace Leap.Data.Queries {
    using System.Threading;
    using System.Threading.Tasks;

    using Leap.Data.Schema;

    public class MultipleKeyQuery<TEntity, TKey> : QueryBase<TEntity>
        where TEntity : class {
        public MultipleKeyQuery(TKey[] keys, Table table) : base(table) {
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