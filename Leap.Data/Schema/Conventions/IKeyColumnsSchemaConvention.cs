namespace Leap.Data.Schema.Conventions {
    using System;
    using System.Collections.Generic;

    public interface IKeyColumnsSchemaConvention : ISchemaConvention {
        IEnumerable<(Type Type, string Name)> GetKeyColumns(Type keyType);
    }
}