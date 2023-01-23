﻿namespace TildeSql {
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;

    public interface IEntityQuery<TEntity> {
        ValueTask<TEntity> SingleAsync<TKey>(TKey key, CancellationToken cancellationToken = default);

        IAsyncEnumerable<TEntity> MultipleAsync<TKey>(IEnumerable<TKey> keys);
    }
}