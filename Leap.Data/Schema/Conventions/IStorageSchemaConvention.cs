namespace Leap.Data.Schema.Conventions {
    using System;
    using System.Collections.Generic;

    public interface IStorageSchemaConvention : ISchemaConvention {
        ITableStorageSettings Configure(string collectionName, HashSet<Type> types);
    }
}