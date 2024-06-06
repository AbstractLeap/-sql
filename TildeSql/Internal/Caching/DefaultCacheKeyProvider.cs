namespace TildeSql.Internal.Caching {
    using System;
    using System.Collections.Generic;
    using System.Text;

    using TildeSql.Queries;
    using TildeSql.Schema;

    public class DefaultCacheKeyProvider : ICacheKeyProvider {
        public string GetEntityCacheKey<TEntity, TKey>(Collection collection, TKey key) {
            var cacheKey = new StringBuilder("key||");
            cacheKey.Append(collection.CollectionName);
            foreach (var keyColumn in collection.KeyColumns) {
                var value = collection.GetKeyColumnValue<TEntity, TKey>(key, keyColumn);
                cacheKey.Append("|").Append(value);
            }
            return cacheKey.ToString();
        }
        public string GetEntityQueryCacheKey<TEntity>(Collection collection, EntityQuery<TEntity> entityQuery) where TEntity : class {
            //throw new System.NotImplementedException();
            var cacheKey = new StringBuilder("query|");
            cacheKey.Append(collection.CollectionName);
            cacheKey.Append("|");
            cacheKey.Append(entityQuery.WhereClause);
            cacheKey.Append("|");
            cacheKey.Append(entityQuery.OrderByClause);
            cacheKey.Append("|");
            cacheKey.Append(entityQuery.Limit);
            cacheKey.Append("|");
            cacheKey.Append(entityQuery.Offset);
            cacheKey.Append("|");
            cacheKey.Append(entityQuery.EntityType);
            cacheKey.Append("|");
            if (entityQuery.WhereClauseParameters != null) {
                foreach (var parameter in entityQuery.WhereClauseParameters) {
                    if (!this.AppendParameter(cacheKey, parameter)) {
                        return null;
                    }
                }
            }
            return cacheKey.ToString();
        }
        public virtual bool AppendParameter(StringBuilder cacheKey, KeyValuePair<string, object> parameter) {
            cacheKey.Append(parameter.Key);
            cacheKey.Append("`");
            if (parameter.Value != null) {
                if (parameter.Value is ValueType) {
                    cacheKey.Append(parameter.Value.ToString());
                }
                else if (parameter.Value is string stringValue) {
                    cacheKey.Append(stringValue);
                }
                else {
                    // TODO support more things (enumerable of value types/strings, things that are 
                    return false;
                }
            }
            else {
                cacheKey.Append("___NULL___");
            }
            return true;
        }
    }
}