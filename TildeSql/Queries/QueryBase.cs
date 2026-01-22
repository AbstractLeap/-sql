namespace TildeSql.Queries {
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;

    using TildeSql.Schema;

    public abstract class QueryBase<TEntity> : IQuery<TEntity>, IQuery, IEquatable<IQuery>
        where TEntity : class {
        private readonly Guid identifier;

        private HashSet<(string, TimeSpan)> calculatedCacheKeys;

        private string explicitCacheKey;

        private TimeSpan? explicitAbsoluteExpirationRelativeToNow;

        private bool? cacheEnabled;

        public QueryBase(Collection collection) {
            this.Collection = collection;
            this.identifier = Guid.NewGuid();
        }

        public virtual Type EntityType => typeof(TEntity);

        public Collection Collection { get; }

        public void EnableCache(string cacheKey, TimeSpan? absoluteExpirationRelativeToNow) {
            this.explicitCacheKey = cacheKey;
            this.explicitAbsoluteExpirationRelativeToNow = absoluteExpirationRelativeToNow;
            this.cacheEnabled = true;
        }

        public void DisableCache() {
            this.cacheEnabled = false;
            this.explicitCacheKey = null;
            this.explicitAbsoluteExpirationRelativeToNow = null;
        }

        internal string ExplicitCacheKey => this.explicitCacheKey;

        internal TimeSpan? ExplicitAbsoluteExpirationRelativeToNow => this.explicitAbsoluteExpirationRelativeToNow;

        internal bool IsCacheDisabled => this.cacheEnabled is false;

        internal void AddResolvedCacheOptions(string cacheKey, TimeSpan expirationRelativeToNow) {
            this.calculatedCacheKeys ??= new();
            this.calculatedCacheKeys.Add((cacheKey, expirationRelativeToNow));
        }

        public bool IsCacheable => this.calculatedCacheKeys != null;

        public IEnumerable<(string cacheKey, TimeSpan absoluteExpirationRelativeToNow)> ResolvedCacheOptions() {
            return this.calculatedCacheKeys ?? [];
        }

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