namespace Leap.Data.Operations {
    using Leap.Data.IdentityMap;

    public class DeleteOperation<TEntity> : IOperation {
        public Document<TEntity> Document { get; }

        public DeleteOperation(Document<TEntity> document) {
            this.Document = document;
        }
    }
}