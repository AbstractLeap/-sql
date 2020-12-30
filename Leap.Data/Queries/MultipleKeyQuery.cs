namespace Leap.Data.Queries {
    class MultipleKeyQuery<TEntity, TKey> : QueryBase<TEntity>
        where TEntity : class {
        public MultipleKeyQuery(TKey[] keys) {
            this.Keys = keys;
        }

        public TKey[] Keys { get; }
    }
}