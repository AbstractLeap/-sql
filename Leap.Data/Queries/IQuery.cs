namespace Leap.Data.Queries {
    using System;

    public interface IQuery {
        Guid Identifier { get; }

        Type EntityType { get; }

        void Accept(IQueryVisitor visitor);
    }

    internal interface IQuery<TEntity> : IQuery
        where TEntity : class { }
}