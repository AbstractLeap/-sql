namespace Leap.Data.Schema {
    using System;

    public interface ICollectionNamingSchemaConvention : ISchemaConvention {
        string GetCollectionName(Type type);
    }
}