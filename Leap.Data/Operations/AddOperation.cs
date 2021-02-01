namespace Leap.Data.Operations {
    using Leap.Data.Schema;

    class AddOperation<TEntity> : IOperation<TEntity> {
        public Table Table { get; }

        public TEntity Entity { get; }

        public AddOperation(TEntity entity, Table table) {
            this.Entity = entity;
            this.Table  = table;
        }
    }
}