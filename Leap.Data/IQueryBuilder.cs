namespace Leap.Data {
    public interface IQueryBuilder<TEntity> : IEntityQuery<TEntity>, IFutureEntityKeyQueryBuilder<TEntity>, IEntityQueryBuilder<TEntity> { }
}