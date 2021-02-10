namespace Leap.Data.Operations {
    using Leap.Data.Schema;

    interface IOperation {
        Collection Collection { get; }
    }

    interface IOperation<out TEntity> : IOperation {
        TEntity Entity { get; }
    }
}