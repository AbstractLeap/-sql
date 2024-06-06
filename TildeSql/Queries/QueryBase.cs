namespace TildeSql.Queries {
    using System;
    using System.Threading;
    using System.Threading.Tasks;

    using TildeSql.Schema;

    public abstract class QueryBase<TEntity> : IQuery<TEntity>, IQuery, IEquatable<IQuery>
        where TEntity : class {
        private readonly Guid identifier;

        public QueryBase(Collection collection) {
            this.Collection = collection;
            this.identifier = Guid.NewGuid();
        }

        public virtual Type EntityType => typeof(TEntity);

        public Collection Collection { get; }

        public bool? CacheQuery { get; set; }

        public TimeSpan? AbsoluteExpirationRelativeToNow { get; set; }

        public string CacheKey { get; set; }

        public abstract void Accept(IQueryVisitor visitor);

        public abstract ValueTask AcceptAsync(IAsyncQueryVisitor visitor, CancellationToken cancellationToken = default);

        public bool Equals(IQuery other) {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            if (!(other is QueryBase<TEntity> otherQueryBase)) {
                return false;
            }

            return this.identifier.Equals(otherQueryBase.identifier);
        }

        public override bool Equals(object obj) {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((QueryBase<TEntity>)obj);
        }

        public override int GetHashCode() {
            return this.identifier.GetHashCode();
        }

        public static bool operator ==(QueryBase<TEntity> left, QueryBase<TEntity> right) {
            return Equals(left, right);
        }

        public static bool operator !=(QueryBase<TEntity> left, QueryBase<TEntity> right) {
            return !Equals(left, right);
        }
    }
}