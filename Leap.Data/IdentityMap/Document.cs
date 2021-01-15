namespace Leap.Data.IdentityMap {
    using Leap.Data.Internal;

    internal class Document<TEntity> : IDocument<TEntity> {
        public Document(TEntity entity) {
            this.Entity = entity;
        }

        public Document(TEntity entity, DatabaseRow row) {
            this.Entity = entity;
            this.Row    = row;
        }

        public TEntity Entity { get; }

        public DatabaseRow Row { get; set; }

        public DocumentState State { get; set; }
    }
}