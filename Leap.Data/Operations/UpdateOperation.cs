namespace Leap.Data.Operations {
    using Leap.Data.IdentityMap;

    public class UpdateOperation<TEntity, TKey> : IOperation {
        public IDocument<TEntity> Document { get; }

        public TKey Key { get; }

        public UpdateOperation(IDocument<TEntity> document, TKey key) {
            this.Document = document;
            this.Key      = key;
        }
    }
}