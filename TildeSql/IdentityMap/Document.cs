namespace TildeSql.IdentityMap {
    using TildeSql.Internal;
    using TildeSql.Schema;

    internal class Document<TEntity> : IDocument<TEntity> {
        public Document(TEntity entity, Collection collection) {
            this.Entity = entity;
            this.Collection  = collection;
        }

        public Document(TEntity entity, DatabaseRow row) {
            this.Entity = entity;
            this.Row    = row;
            this.Collection  = row.Collection;
        }

        public TEntity Entity { get; }

        public Collection Collection { get; }

        public DatabaseRow Row { get; set; }

        public DocumentState State { get; set; }
    }
}