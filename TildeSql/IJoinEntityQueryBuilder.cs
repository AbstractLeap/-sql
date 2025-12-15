namespace TildeSql {
    public interface IJoinEntityQueryBuilder<TEntity> : IEntityQueryBuilder<TEntity> {
        public IEntityQueryBuilder<TEntity> On(string searchCondition);
    }
}