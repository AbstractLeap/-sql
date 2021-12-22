namespace TildeSql.Schema.Conventions {
    using System;
    using System.Collections.Generic;

    public interface IKeyComputedSchemaConvention : ISchemaConvention {
        bool IsKeyComputed(string collectionName, IEnumerable<Type> entityTypes);
    }
}