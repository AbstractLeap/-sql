namespace TildeSql.Infrastructure {
    using Humanizer;

    using TildeSql.Schema.Conventions;

    public class CollectionNamingSchemaConvention : ICollectionNamingSchemaConvention {
        public string GetCollectionName(Type type) {
            var name = type.Name;
            if (type.IsGenericTypeDefinition) {
                var backTickIdx = name.IndexOf('`');
                if (backTickIdx > -1) {
                    name = name.Remove(backTickIdx);
                }
            }

            if (name.EndsWith("Wrapper")) {
                return name.Substring(0, name.Length - "Wrapper".Length).Pluralize();
            }

            return name.Pluralize();
        }
    }
}