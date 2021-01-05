namespace Leap.Data.Operations {
    public class AddOperation<TEntity> : IOperation {
        public TEntity Entity { get; }

        public AddOperation(TEntity entity) {
            this.Entity = entity;
        }
    }
}