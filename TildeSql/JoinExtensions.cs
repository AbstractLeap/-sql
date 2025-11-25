namespace TildeSql {
    public static class EntityQueryBuilderJoinExtensions {
        public static IEntityQueryBuilder<TEntity> Join<TEntity>(this IEntityQueryBuilder<TEntity> queryBuilder, string joinType, string tableSource, string searchCondition) {
            if (!string.IsNullOrWhiteSpace(searchCondition))
                return queryBuilder.Join($"{joinType} {tableSource} on {searchCondition}");
            return queryBuilder.Join($"{joinType} {tableSource}");
        }

        public static IEntityQueryBuilder<TEntity> LeftJoin<TEntity>(this IEntityQueryBuilder<TEntity> queryBuilder, string tableSource, string searchCondition) {
            return queryBuilder.Join("left join", tableSource, searchCondition);
        }

        public static IEntityQueryBuilder<TEntity> RightJoin<TEntity>(this IEntityQueryBuilder<TEntity> queryBuilder, string tableSource, string searchCondition) {
            return queryBuilder.Join("right join", tableSource, searchCondition);
        }

        public static IEntityQueryBuilder<TEntity> OuterJoin<TEntity>(this IEntityQueryBuilder<TEntity> queryBuilder, string tableSource, string searchCondition) {
            return queryBuilder.Join("full outer join", tableSource, searchCondition);
        }

        public static IEntityQueryBuilder<TEntity> InnerJoin<TEntity>(this IEntityQueryBuilder<TEntity> queryBuilder, string tableSource, string searchCondition) {
            return queryBuilder.Join("inner join", tableSource, searchCondition);
        }

        public static IEntityQueryBuilder<TEntity> CrossJoin<TEntity>(this IEntityQueryBuilder<TEntity> queryBuilder, string tableSource) {
            return queryBuilder.Join("cross join", tableSource, null);
        }

        public static IEntityQueryBuilder<TEntity> CrossApply<TEntity>(this IEntityQueryBuilder<TEntity> queryBuilder, string tableSource) {
            return queryBuilder.Join("cross apply", tableSource, null);
        }

        public static IEntityQueryBuilder<TEntity> OuterApply<TEntity>(this IEntityQueryBuilder<TEntity> queryBuilder, string tableSource) {
            return queryBuilder.Join("outer apply", tableSource, null);
        }
    }
}