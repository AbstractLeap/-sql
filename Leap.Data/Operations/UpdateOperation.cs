namespace Leap.Data.Operations {
    using Leap.Data.IdentityMap;

    class UpdateOperation<TEntity> : IOperation<TEntity> {
        public IDocument<TEntity> Document { get; }

        public UpdateOperation(IDocument<TEntity> document) {
            this.Document = document;
        }
    }
}