namespace Leap.Data.IdentityMap {
    using System.Collections.Generic;

    using Leap.Data.Schema;

    class IdentityMap {
        private readonly ISchema schema;

        private readonly IDictionary<Table, object> map = new Dictionary<Table, object>();

        public IdentityMap(ISchema schema) {
            this.schema = schema;
        }

        public bool TryGetValue<TEntity, TKey>(TKey key, out TEntity entity)
            where TEntity : class {
            var table = this.schema.GetTable<TEntity>();
            if (this.map.TryGetValue(table, out var entityMap)) {
                return ((IDictionary<TKey, TEntity>)entityMap).TryGetValue(key, out entity);
            }

            entity = null;
            return false;
        }

        public void Add<TEntity, TKey>(TKey key, TEntity entity) {
            var table = this.schema.GetTable<TEntity>();
            if (this.map.TryGetValue(table, out var entityMap)) {
                ((IDictionary<TKey, TEntity>)entityMap).Add(key, entity);
            }
            else {
                var typedMap = new Dictionary<TKey, TEntity>();
                typedMap.Add(key, entity);
                this.map.Add(table, typedMap);
            }
        }

        public void Remove<TEntity, TKey>(TKey key) {
            var table = this.schema.GetTable<TEntity>();
            if (this.map.TryGetValue(table, out var entityMap)) {
                ((IDictionary<TKey, TEntity>)entityMap).Remove(key);
            }
        }
    }
}