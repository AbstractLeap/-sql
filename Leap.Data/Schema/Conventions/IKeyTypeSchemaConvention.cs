namespace Leap.Data.Schema.Conventions {
    using System;
    using System.Collections.Generic;

    public interface IKeyTypeSchemaConvention : ISchemaConvention {
        Type GetKeyType(string tableName, IEnumerable<Type> entityTypes);
    }
}