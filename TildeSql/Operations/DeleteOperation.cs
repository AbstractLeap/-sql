﻿namespace TildeSql.Operations {
    using TildeSql.Schema;

    class DeleteOperation<TEntity> : IOperation<TEntity> {
        public TEntity Entity { get; }

        public Collection Collection { get; }

        public DeleteOperation(TEntity entity, Collection collection) {
            this.Entity = entity;
            this.Collection  = collection;
        }
    }
}