namespace Leap.Data.Schema.Conventions {
    using System;
    using System.Collections.Generic;

    public interface IKeyTypeSchemaConvention : ISchemaConvention {
        Type GetKeyType(string collectionName, IEnumerable<Type> entityTypes);
    }
}