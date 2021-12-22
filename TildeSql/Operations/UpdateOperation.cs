namespace TildeSql.Operations {
    using TildeSql.Schema;

    class UpdateOperation<TEntity> : IOperation<TEntity> {
        public TEntity Entity { get; }

        public Collection Collection { get; }

        public UpdateOperation(TEntity entity, Collection collection) {
            this.Entity = entity;
            this.Collection  = collection;
        }
    }
}