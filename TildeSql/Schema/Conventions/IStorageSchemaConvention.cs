namespace TildeSql.Schema.Conventions {
    using System;
    using System.Collections.Generic;

    public interface IStorageSchemaConvention : ISchemaConvention {
        ICollectionStorageSettings Configure(string collectionName, HashSet<Type> types);
    }
}