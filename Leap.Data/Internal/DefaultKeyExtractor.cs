namespace Leap.Data.Internal {
    using System;
    using System.Collections.Concurrent;

    using Leap.Data.Schema;

    class DefaultKeyExtractor : IKeyExtractor {
        private readonly ISchema schema;

        private Type matchingEntityType;

        private object typedKeyExtractor;

        private ConcurrentDictionary<Type, object> extractorLookup;

        public DefaultKeyExtractor(ISchema schema) {
            this.schema = schema;
        }

        public TKey Extract<TEntity, TKey>(TEntity entity) {
            var entityType = typeof(TEntity);
            // generally you'll have one type per table so this is a hot path (and quick)
            if (this.typedKeyExtractor != null && this.matchingEntityType == entityType) {
                return ((DefaultTypedKeyExtractor<TEntity, TKey>)this.typedKeyExtractor).Extract(entity);
            }

            // not set up yet
            if (this.matchingEntityType == null) {
                this.matchingEntityType = entityType;
                var defaultTypedKeyExtractor = new DefaultTypedKeyExtractor<TEntity, TKey>();
                this.typedKeyExtractor = defaultTypedKeyExtractor;
                return defaultTypedKeyExtractor.Extract(entity);
            }

            // multiple types for this table
            if (this.extractorLookup == null) {
                this.extractorLookup = new ConcurrentDictionary<Type, object>();
            }

            if (this.extractorLookup.TryGetValue(entityType, out var keyExtractor)) {
                return ((DefaultTypedKeyExtractor<TEntity, TKey>)keyExtractor).Extract(entity);
            }

            var newDefaultTypedKeyExtractor = new DefaultTypedKeyExtractor<TEntity, TKey>();
            this.extractorLookup.TryAdd(entityType, newDefaultTypedKeyExtractor);
            return newDefaultTypedKeyExtractor.Extract(entity);
        }
    }
}