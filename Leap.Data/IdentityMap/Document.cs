namespace Leap.Data.IdentityMap {
    using Leap.Data.Internal;

    class Document<TEntity> {
        public TEntity Entity { get; init; }
        
        public DatabaseRow Row { get; set; }
        
        public DocumentState State { get; set; }
    }
}