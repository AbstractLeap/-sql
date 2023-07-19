namespace TildeSql.Infrastructure {
    using TildeSql.Schema.Conventions;

    public class KeyComputedSchemaConvention : IKeyComputedSchemaConvention {
        public bool IsKeyComputed(string collectionName, IEnumerable<Type> entityTypes) {
            return collectionName == "DomainEvents";
        }
    }
}