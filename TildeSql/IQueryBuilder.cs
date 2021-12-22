namespace TildeSql {
    public interface IQueryBuilder<TEntity> : IEntityQuery<TEntity>, IFutureEntityKeyQueryBuilder<TEntity>, IEntityQueryBuilder<TEntity> { }
}