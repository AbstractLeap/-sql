namespace Leap.Data.IdentityMap {
    using Leap.Data.Internal;

    internal class Document<TEntity> : IDocument<TEntity> {
        public Document(DatabaseRow row) {
            this.Row = row;
        }

        public Document(DatabaseRow row, TEntity entity) {
            this.Row    = row;
            this.Entity = entity;
        }

        public TEntity Entity { get; }

        public DatabaseRow Row { get; }

        public DocumentState State { get; set; }
    }
}