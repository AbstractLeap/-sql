namespace Leap.Data.Internal {
    using System;
    using System.Collections.Generic;

    using Fasterflect;

    using Leap.Data.Schema;
    using Leap.Data.Utilities;

    class DefaultKeyColumnValueExtractor : IKeyColumnValueExtractor {
        private readonly ISchema schema;

        public DefaultKeyColumnValueExtractor(ISchema schema) {
            this.schema = schema;
        }

        public IDictionary<Column, object> Extract<TEntity, TKey>(TKey key) {
            // TODO caching, move somewhere or make singleton
            var table = this.schema.GetTable<TEntity>();
            var result = new Dictionary<Column, object>(table.KeyColumns.Count);
            foreach (var columnEntry in table.KeyColumns.AsSmartEnumerable()) {
                var fieldInfo = typeof(TKey).Field(columnEntry.Value.Name);
                if (fieldInfo != null) {
                    result[columnEntry.Value] = fieldInfo.Get(key);
                }
                else {
                    var propertyInfo = typeof(TKey).Property(columnEntry.Value.Name);
                    if (propertyInfo != null) {
                        result[columnEntry.Value] = propertyInfo.Get(key);
                    }
                    else {
                        throw new Exception($"Unable to extract value named {columnEntry.Value.Name} from {typeof(TKey)}");
                    }
                }
            }

            return result;
        }
    }
}