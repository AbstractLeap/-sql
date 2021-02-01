namespace Leap.Data.Operations {
    using Leap.Data.Schema;

    class UpdateOperation<TEntity> : IOperation<TEntity> {
        public TEntity Entity { get; }

        public Table Table { get; }

        public UpdateOperation(TEntity entity, Table table) {
            this.Entity = entity;
            this.Table  = table;
        }
    }
}