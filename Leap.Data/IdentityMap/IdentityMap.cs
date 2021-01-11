namespace Leap.Data.IdentityMap {
    using System;
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
                if (((IDictionary<TKey, object>)entityMap).TryGetValue(key, out var nonTypedDocument)) {
                    if (nonTypedDocument is Document<TEntity> typedDocument) {
                        document = typedDocument;
                        return true;
                    }
                    else {
                        throw new Exception($"Unable to cast document to {typeof(TEntity)}");
                    }
                }
            }

            document = null;
            return false;
        }

        public void Add<TEntity, TKey>(TKey key, Document<TEntity> document) {
            var table = this.schema.GetTable<TEntity>();
            if (this.map.TryGetValue(table, out var entityMap)) {
                ((IDictionary<TKey, object>)entityMap).Add(key, document);
            }
            else {
                var typedMap = new Dictionary<TKey, object>();
                typedMap.Add(key, document);
                this.map.Add(table, typedMap);
            }
        }

        public void Remove<TEntity, TKey>(TKey key) {
            var table = this.schema.GetTable<TEntity>();
            if (this.map.TryGetValue(table, out var entityMap)) {
                ((IDictionary<TKey, object>)entityMap).Remove(key);
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