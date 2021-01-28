namespace Leap.Data {
    using System;

    using Fasterflect;

    using Leap.Data.IdentityMap;
    using Leap.Data.Internal;
    using Leap.Data.Schema;

    public interface IEntityInspector<TEntity> {
        T GetColumnValue<T>(string columnName);
    }

    class EntityInspector<TEntity> : IEntityInspector<TEntity>
        where TEntity : class {
        private readonly ISchema schema;

        private readonly IdentityMap.IdentityMap identityMap;

        private readonly TEntity entity;

        public EntityInspector(ISchema schema, IdentityMap.IdentityMap identityMap, TEntity entity) {
            this.schema      = schema;
            this.identityMap = identityMap;
            this.entity      = entity;
        }

        public T GetColumnValue<T>(string columnName) {
            var table = this.schema.GetTable<TEntity>();
            var keyType = table.KeyType;
            var key = table.KeyExtractor.CallMethod(new[] { typeof(TEntity), keyType }, nameof(IKeyExtractor.Extract), this.entity);
            if (!this.identityMap.TryGetValue<TEntity>(keyType, key, out var document)) {
                throw new Exception($"The entity {this.entity} is not attached to this session");
            }

            var colIdx = table.GetColumnIndex(columnName);
            var obj = document.Row.Values[colIdx];
            if (!(obj is T typedObj)) {
                throw new Exception($"Unable to cast object to type {typeof(T)}");
            }

            return typedObj;
        }
    }
}