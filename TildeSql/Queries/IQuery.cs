namespace TildeSql.Queries {
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;

    using TildeSql.Schema;

    public interface IQuery {
        Type EntityType { get; }
        
        Collection Collection { get; }

        bool IsCacheable { get; }

        bool NotTracked { get; }

        IEnumerable<(string cacheKey, TimeSpan absoluteExpirationRelativeToNow)> ResolvedCacheOptions();

        void Accept(IQueryVisitor visitor);

        ValueTask AcceptAsync(IAsyncQueryVisitor visitor, CancellationToken cancellationToken = default);
    }

    internal interface IQuery<TEntity> : IQuery
        where TEntity : class { }
}