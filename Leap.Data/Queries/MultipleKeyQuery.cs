﻿namespace Leap.Data.Queries {
    public class MultipleKeyQuery<TEntity, TKey> : QueryBase<TEntity>
        where TEntity : class {
        public MultipleKeyQuery(TKey[] keys) {
            this.Keys = keys;
        }

        public TKey[] Keys { get; }

        public override void Accept(IQueryVisitor visitor)
        {
            visitor.VisitMultipleKeyQuery(this);
        }
    }
}