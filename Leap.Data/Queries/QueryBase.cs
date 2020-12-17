namespace Leap.Data.Queries {
    using System;

    abstract class QueryBase : IQuery {
        public QueryBase() {
            this.Identifier = Guid.NewGuid();
        }
        
        public Guid Identifier { get; }

        public abstract Type EntityType { get; }
    }
}