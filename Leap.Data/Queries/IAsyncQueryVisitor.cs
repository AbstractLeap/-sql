namespace Leap.Data.Queries {
    using System.Threading;
    using System.Threading.Tasks;

    public interface IAsyncQueryVisitor {
        ValueTask VisitEntityQueryAsync<TEntity>(EntityQuery<TEntity> entityQuery, CancellationToken cancellationToken = default) where TEntity : class;

        ValueTask VisitKeyQueryAsync<TEntity, TKey>(KeyQuery<TEntity, TKey> keyQuery, CancellationToken cancellationToken = default) where TEntity : class;

        ValueTask VisitMultipleKeyQueryAsync<TEntity, TKey>(MultipleKeyQuery<TEntity, TKey> multipleKeyQuery, CancellationToken cancellationToken = default) where TEntity : class;
    }
}