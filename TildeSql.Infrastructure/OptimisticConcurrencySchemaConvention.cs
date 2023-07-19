namespace TildeSql.Infrastructure {
    using TildeSql.Schema.Conventions;

    public class OptimisticConcurrencySchemaConvention : IOptimisticConcurrencySchemaConvention {
        public bool UseOptimisticConcurrency(string collectionName, IEnumerable<Type> entityTypes) {
            return collectionName != "DomainEvents";
        }
    }
}