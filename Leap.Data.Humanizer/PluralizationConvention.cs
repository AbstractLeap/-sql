namespace Leap.Data.Humanizer {
    using System;

    using global::Humanizer;

    using Leap.Data.Schema;

    public class PluralizationConvention : INamingSchemaConvention {
        public string GetTableName(Type type) {
            return type.Name.Pluralize();
        }
    }
}