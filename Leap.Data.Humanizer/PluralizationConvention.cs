namespace Leap.Data.Humanizer {
    using System;

    using global::Humanizer;

    using Leap.Data.Schema;

    public class PluralizationConvention : INamingSchemaConvention, ICollectionNamingSchemaConvention {
        public string GetTableName(Type type) {
            return PluralizeTypeName(type);
        }

        public string GetCollectionName(Type type) {
            return PluralizeTypeName(type);
        }

        private static string PluralizeTypeName(Type type) {
            return type.Name.Pluralize();
        }
    }
}