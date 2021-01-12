namespace Leap.Data.Operations {
    using Leap.Data.IdentityMap;

    public class DeleteOperation<TEntity> : IOperation {
        public IDocument<TEntity> Document { get; }

        public DeleteOperation(IDocument<TEntity> document) {
            this.Document = document;
        }
    }
}