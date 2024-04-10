﻿namespace TildeSql.Queries {
    using System;
    using System.Threading;
    using System.Threading.Tasks;

    using TildeSql.Schema;

    public interface IQuery {
        Type EntityType { get; }
        
        Collection Collection { get; }

        void Accept(IQueryVisitor visitor);

        ValueTask AcceptAsync(IAsyncQueryVisitor visitor, CancellationToken cancellationToken = default);
    }

    internal interface IQuery<TEntity> : IQuery
        where TEntity : class { }

    internal interface IMultipleKeyQuery : IQuery {
        object[] ExpectedKeys();
    }
}