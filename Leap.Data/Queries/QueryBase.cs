namespace Leap.Data.Queries {
    using System;
    using System.Threading;
    using System.Threading.Tasks;

    using Leap.Data.Schema;

    public abstract class QueryBase<TEntity> : IQuery<TEntity>, IQuery
        where TEntity : class {
        public QueryBase(Collection collection) {
            this.Collection      = collection;
            this.Identifier = Guid.NewGuid();
        }

        public Guid Identifier { get; }

        public virtual Type EntityType => typeof(TEntity);

        public Collection Collection { get; }

        public abstract void Accept(IQueryVisitor visitor);

        public abstract ValueTask AcceptAsync(IAsyncQueryVisitor visitor, CancellationToken cancellationToken = default);
    }
}