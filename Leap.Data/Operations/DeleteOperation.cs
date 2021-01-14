namespace Leap.Data.Operations {
    using Leap.Data.IdentityMap;

    class DeleteOperation<TEntity> : IOperation<TEntity> {
        public IDocument<TEntity> Document { get; }

        public DeleteOperation(IDocument<TEntity> document) {
            this.Document = document;
        }
    }
}