namespace Leap.Data.Schema.Conventions {
    using System;

    public interface ICollectionNamingSchemaConvention : ISchemaConvention {
        string GetCollectionName(Type type);
    }
}