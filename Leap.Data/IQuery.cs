namespace Leap.Data {
    using System;

    internal interface IQuery {
        Guid Identifier { get; }
        
        Type EntityType { get; }
    }

    abstract class QueryBase : IQuery {
        public QueryBase() {
            this.Identifier = Guid.NewGuid();
        }
        
        public Guid Identifier { get; }

        public abstract Type EntityType { get; }
    }

    class KeyQuery<TEntity, TKey> : QueryBase {
        public KeyQuery(TKey key) {
            this.Key = key;
        }
        
        public TKey Key { get; }

        public override Type EntityType => typeof(TEntity);
    } 
}