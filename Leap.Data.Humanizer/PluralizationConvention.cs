namespace Leap.Data.Humanizer {
    using System;

    using global::Humanizer;

    using Leap.Data.Schema.Conventions;

    public class PluralizationConvention : ICollectionNamingSchemaConvention {
        public string GetCollectionName(Type type) {
            return type.Name.Pluralize();
        }
    }
}