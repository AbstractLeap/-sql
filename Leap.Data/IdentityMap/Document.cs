namespace Leap.Data.IdentityMap {
    using Leap.Data.Internal;
    using Leap.Data.Schema;

    internal class Document<TEntity> : IDocument<TEntity> {
        public Document(TEntity entity, Table table) {
            this.Entity = entity;
            this.Table  = table;
        }

        public Document(TEntity entity, DatabaseRow row) {
            this.Entity = entity;
            this.Row    = row;
            this.Table  = row.Table;
        }

        public TEntity Entity { get; }

        public Table Table { get; }

        public DatabaseRow Row { get; set; }

        public DocumentState State { get; set; }
    }
}