namespace Leap.Data.Operations {
    using Leap.Data.Schema;

    class DeleteOperation<TEntity> : IOperation<TEntity> {
        public TEntity Entity { get; }

        public Table Table { get; }

        public DeleteOperation(TEntity entity, Table table) {
            this.Entity = entity;
            this.Table  = table;
        }
    }
}