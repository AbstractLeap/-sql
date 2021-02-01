namespace Leap.Data.Operations {
    using Leap.Data.Schema;

    interface IOperation {
        Table Table { get; }
    }

    interface IOperation<out TEntity> : IOperation {
        TEntity Entity { get; }
    }
}