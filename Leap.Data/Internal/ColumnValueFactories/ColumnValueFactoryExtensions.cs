namespace Leap.Data.Internal.ColumnValueFactories {
    using System;

    using Fasterflect;

    using Leap.Data.IdentityMap;
    using Leap.Data.Schema.Columns;

    static class ColumnValueFactoryExtensions {
        public static object GetValue<TEntity, TKey>(this IColumnValueFactory columnValueFactory, Column column, TEntity entity, IDocument<TEntity> document) {
            return columnValueFactory.CallMethod(new Type[] { typeof(TEntity), typeof(TKey), column.Type }, nameof(IColumnValueFactory.GetValue), new Type[] { typeof(Column), typeof(TEntity), typeof(IDocument<>).MakeGenericType(typeof(TEntity)) }, column, entity, document);
        }
    }
}