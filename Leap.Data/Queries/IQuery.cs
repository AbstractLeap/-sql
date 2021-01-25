namespace Leap.Data.Queries {
    using System;
    using System.Threading;
    using System.Threading.Tasks;

    public interface IQuery {
        Guid Identifier { get; }

        Type EntityType { get; }

        void Accept(IQueryVisitor visitor);

        ValueTask AcceptAsync(IAsyncQueryVisitor visitor, CancellationToken cancellationToken = default);
    }

    internal interface IQuery<TEntity> : IQuery
        where TEntity : class { }
}