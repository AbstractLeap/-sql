namespace Leap.Data.Operations {
    using Leap.Data.IdentityMap;

    class AddOperation<TEntity> : IOperation<TEntity> {
        public IDocument<TEntity> Document { get; }

        public AddOperation(IDocument<TEntity> document) {
            this.Document = document;
        }
    }
}