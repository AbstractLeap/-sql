namespace Leap.Data.Queries {
    using System;

    class KeyQuery<TEntity, TKey> : QueryBase {
        public KeyQuery(TKey key) {
            this.Key = key;
        }
        
        public TKey Key { get; }

        public override Type EntityType => typeof(TEntity);
    }
}