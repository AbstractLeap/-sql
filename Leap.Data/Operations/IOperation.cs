namespace Leap.Data.Operations {
    using Leap.Data.IdentityMap;

    interface IOperation { }
    
    interface IOperation<out TEntity> : IOperation {
        IDocument<TEntity> Document { get; }
    }
}