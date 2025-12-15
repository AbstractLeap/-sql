namespace TildeSql {
    using System;
    using System.Collections.Generic;
    using System.Threading;

    public static class EntityQueryBuilderJoinExtensions {
        public static IEntityQueryBuilder<TEntity> Join<TEntity>(this IEntityQueryBuilder<TEntity> queryBuilder, string joinType, string tableSource, string searchCondition) {
            if (!string.IsNullOrWhiteSpace(searchCondition))
                return queryBuilder.Join($"{joinType} {tableSource} on {searchCondition}");
            return queryBuilder.Join($"{joinType} {tableSource}");
        }

        public static IJoinEntityQueryBuilder<TEntity> LeftJoin<TEntity>(this IEntityQueryBuilder<TEntity> queryBuilder, string tableSource) {
            return new JoinEntityQueryBuilder<TEntity>(queryBuilder, "left join", tableSource);
        }

        public static IJoinEntityQueryBuilder<TEntity> RightJoin<TEntity>(this IEntityQueryBuilder<TEntity> queryBuilder, string tableSource) {
            return new JoinEntityQueryBuilder<TEntity>(queryBuilder, "right join", tableSource);
        }

        public static IJoinEntityQueryBuilder<TEntity> OuterJoin<TEntity>(this IEntityQueryBuilder<TEntity> queryBuilder, string tableSource) {
            return new JoinEntityQueryBuilder<TEntity>(queryBuilder, "full outer join", tableSource);
        }

        public static IJoinEntityQueryBuilder<TEntity> InnerJoin<TEntity>(this IEntityQueryBuilder<TEntity> queryBuilder, string tableSource) {
            return new JoinEntityQueryBuilder<TEntity>(queryBuilder, "inner join", tableSource);
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

        class JoinEntityQueryBuilder<TEntity> : IJoinEntityQueryBuilder<TEntity> {
            private readonly IEntityQueryBuilder<TEntity> baseQueryBuilder;

            private readonly string joinType;

            private readonly string tableSource;

            public JoinEntityQueryBuilder(IEntityQueryBuilder<TEntity> baseQueryBuilder, string joinType, string tableSource) {
                this.baseQueryBuilder = baseQueryBuilder;
                this.joinType = joinType;
                this.tableSource = tableSource;
            }

            public IAsyncEnumerator<TEntity> GetAsyncEnumerator(CancellationToken cancellationToken = new CancellationToken()) {
                return this.ApplyJoinWithoutSearchCondition().GetAsyncEnumerator(cancellationToken);
            }

            public IEntityQueryBuilder<TEntity> Where(string whereClause, IDictionary<string, object> parameters = null) {
                return this.ApplyJoinWithoutSearchCondition().Where(whereClause, parameters);
            }

            public IEntityQueryBuilder<TEntity> Where(string whereClause, object parameters) {
                return this.ApplyJoinWithoutSearchCondition().Where(whereClause, parameters);
            }

            public IEntityQueryBuilder<TEntity> Join(string joinClause) {
                return this.ApplyJoinWithoutSearchCondition().Join(joinClause);
            }

            public IEntityQueryBuilder<TEntity> OrderBy(string orderByClause) {
                return this.ApplyJoinWithoutSearchCondition().OrderBy(orderByClause);
            }

            public IEntityQueryBuilder<TEntity> Offset(int offset) {
                return this.ApplyJoinWithoutSearchCondition().Offset(offset);
            }

            public IEntityQueryBuilder<TEntity> Limit(int limit) {
                return this.ApplyJoinWithoutSearchCondition().Limit(limit);
            }

            public IEntityQueryBuilder<TEntity> Limit(int limit, out ICountAccessor countAccessor) {
                return this.ApplyJoinWithoutSearchCondition().Limit(limit, out countAccessor);
            }

            public IEntityQueryBuilder<TEntity> Cache(TimeSpan? absoluteExpirationRelativeToNow = null, string key = null) {
                return this.ApplyJoinWithoutSearchCondition().Cache(absoluteExpirationRelativeToNow, key);
            }

            public IEntityQueryBuilder<TEntity> NoCache() {
                return this.ApplyJoinWithoutSearchCondition().NoCache();
            }

            public IFutureEntityQueryResult<TEntity> Future() {
                return this.ApplyJoinWithoutSearchCondition().Future();
            }

            private IEntityQueryBuilder<TEntity> ApplyJoinWithoutSearchCondition() {
                return this.baseQueryBuilder.Join(this.joinType, this.tableSource, string.Empty);
            }

            public IEntityQueryBuilder<TEntity> On(string searchCondition) {
                return this.baseQueryBuilder.Join(this.joinType, this.tableSource, searchCondition);
            }
        }
    }
}