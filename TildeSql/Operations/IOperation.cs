namespace TildeSql.Operations {
    using TildeSql.Schema;

    interface IOperation {
        Collection Collection { get; }
    }

    interface IOperation<out TEntity> : IOperation {
        TEntity Entity { get; }
    }
}