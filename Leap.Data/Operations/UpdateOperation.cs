namespace Leap.Data.Operations {
    using Leap.Data.IdentityMap;

    internal class UpdateOperation<TEntity, TKey> : IOperation {
        public Document<TEntity> Document { get; }

        public TKey Key { get; }

        public UpdateOperation(Document<TEntity> document, TKey key) {
            this.Document = document;
            this.Key      = key;
        }
    }
}