namespace Leap.Data.IdentityMap {
    using System.Collections;
    using System.Collections.Generic;

    using Leap.Data.Schema;

    class IdentityMap {
        private readonly ISchema schema;

        private readonly IDictionary<Table, object> map = new Dictionary<Table, object>();

        public IdentityMap(ISchema schema) {
            this.schema = schema;
        }

        public bool TryGetValue<TEntity, TKey>(TKey key, out Document<TEntity> document)
            where TEntity : class {
            var table = this.schema.GetTable<TEntity>();
            if (this.map.TryGetValue(table, out var entityMap)) {
                return ((IDictionary<TKey, Document<TEntity>>)entityMap).TryGetValue(key, out document);
            }

            document = null;
            return false;
        }

        public void Add<TEntity, TKey>(TKey key, Document<TEntity> document) {
            var table = this.schema.GetTable<TEntity>();
            if (this.map.TryGetValue(table, out var entityMap)) {
                ((IDictionary<TKey, Document<TEntity>>)entityMap).Add(key, document);
            }
            else {
                var typedMap = new Dictionary<TKey, Document<TEntity>>();
                typedMap.Add(key, document);
                this.map.Add(table, typedMap);
            }
        }

        public void Remove<TEntity, TKey>(TKey key) {
            var table = this.schema.GetTable<TEntity>();
            if (this.map.TryGetValue(table, out var entityMap)) {
                ((IDictionary<TKey, Document<TEntity>>)entityMap).Remove(key);
            }
        }

        internal IEnumerable<(Table Table, object Document, object Key)> GetAll() {
            foreach (var o in this.map) {
                foreach (DictionaryEntry entry in (IDictionary)o.Value) {
                    yield return (o.Key, entry.Value, entry.Key);
                }
            }
        }
    }
}