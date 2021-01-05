namespace Leap.Data.Queries {
    using System;

    public interface IQuery {
        Guid Identifier { get; }

        Type EntityType { get; }
    }

    internal interface IQuery<TEntity> : IQuery
        where TEntity : class { }
}