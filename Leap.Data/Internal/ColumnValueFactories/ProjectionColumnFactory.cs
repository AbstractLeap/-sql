﻿namespace Leap.Data.Internal.ColumnValueFactories {
    using System;

    using Leap.Data.IdentityMap;
    using Leap.Data.Schema.Columns;

    class ProjectionColumnFactory : IColumnValueFactory {
        public TValue GetValue<TEntity, TKey, TValue>(Column column, TEntity entity, IDocument<TEntity> document) {
            if (!(column is ProjectionColumn<TEntity, TValue> projectionColumn)) {
                throw new Exception($"column is not a projection");
            }

            return projectionColumn.ProjectionFunc(entity);
        }
    }
}