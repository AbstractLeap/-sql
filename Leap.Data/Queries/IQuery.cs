namespace Leap.Data.Queries {
    using System;

    internal interface IQuery {
        Guid Identifier { get; }

        Type EntityType { get; }
    }

    internal interface IQuery<TEntity> : IQuery
        where TEntity : class { }
}