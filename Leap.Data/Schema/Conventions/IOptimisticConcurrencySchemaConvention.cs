namespace Leap.Data.Schema.Conventions {
    using System;
    using System.Collections.Generic;

    public interface IOptimisticConcurrencySchemaConvention : ISchemaConvention {
        bool UseOptimisticConcurrency(string collectionName, IEnumerable<Type> entityTypes);
    }
}