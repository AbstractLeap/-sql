namespace Leap.Data.Schema {
    using System;

    public interface INamingSchemaConvention : ISchemaConvention {
        string GetTableName(Type type);
    }
}