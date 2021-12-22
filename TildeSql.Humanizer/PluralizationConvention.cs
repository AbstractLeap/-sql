namespace TildeSql.Humanizer {
    using System;

    using global::Humanizer;

    using TildeSql.Schema.Conventions;

    public class PluralizationConvention : ICollectionNamingSchemaConvention {
        public string GetCollectionName(Type type) {
            return type.Name.Pluralize();
        }
    }
}