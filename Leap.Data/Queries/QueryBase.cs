namespace Leap.Data.Queries {
    using System;

    abstract class QueryBase<TEntity> : IQuery<TEntity>, IQuery
        where TEntity : class {
        public QueryBase() {
            this.Identifier = Guid.NewGuid();
        }

        public Guid Identifier { get; }

        public virtual Type EntityType => typeof(TEntity);
    }
}