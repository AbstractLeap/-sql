namespace Leap.Data.Operations {
    using Leap.Data.Schema;

    class AddOperation<TEntity> : IOperation<TEntity> {
        public Collection Collection { get; }

        public TEntity Entity { get; }

        public AddOperation(TEntity entity, Collection collection) {
            this.Entity = entity;
            this.Collection  = collection;
        }
    }
}