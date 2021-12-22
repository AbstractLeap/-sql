namespace TildeSql.IdentityMap {
    using System.Collections.Generic;

    class IdentityMap {
        private readonly Dictionary<object, HashSet<object>> map = new();

        public bool TryGetValue<TEntity, TKey>(TKey key, out TEntity entity)
            where TEntity : class {
            if (!this.map.TryGetValue(key, out var entityList)) {
                entity = null;
                return false;
            }

            foreach (var storedDocument in entityList) {
                if (storedDocument is TEntity typedEntity) {
                    entity = typedEntity;
                    return true;
                }
            }

            entity = null;
            return false;
        }

        public void Add<TEntity, TKey>(TKey key, TEntity entity) {
            if (this.map.TryGetValue(key, out var entityList)) {
                entityList.Add(entity);
            }
            else {
                this.map.Add(key, new HashSet<object> { entity });
            }
        }
    }
}