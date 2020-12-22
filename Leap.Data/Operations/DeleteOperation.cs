namespace Leap.Data.Operations {
    internal class DeleteOperation<TEntity> : IOperation {
        public TEntity Entity { get; }

        public DeleteOperation(TEntity entity) {
            this.Entity = entity;
        }
    }
}