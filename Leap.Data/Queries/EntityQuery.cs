namespace Leap.Data.Queries {
    class EntityQuery<TEntity> : QueryBase<TEntity>
        where TEntity : class {
        public string WhereClause { get; set; }

        public string OrderByClause { get; set; }

        public int? Limit { get; set; }

        public int? Offset { get; set; }
    }
}