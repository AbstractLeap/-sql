namespace Leap.Data.Queries {
    public class KeyQuery<TEntity, TKey> : QueryBase<TEntity>
        where TEntity : class {
        public KeyQuery(TKey key) {
            this.Key = key;
        }

        public TKey Key { get; }

        public override void Accept(IQueryVisitor visitor)
        {
            visitor.VisitKeyQuery(this);
        }
    }
}