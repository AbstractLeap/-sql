namespace Leap.Data.Queries {
    class KeyQuery<TEntity, TKey> : QueryBase<TEntity>
        where TEntity : class {
        public KeyQuery(TKey key) {
            this.Key = key;
        }

        public TKey Key { get; }
    }
}