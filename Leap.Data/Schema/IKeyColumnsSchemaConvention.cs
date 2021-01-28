namespace Leap.Data.Schema {
    using System;
    using System.Collections.Generic;

    public interface IKeyColumnsSchemaConvention : ISchemaConvention {
        IEnumerable<(Type Type, string Name)> GetKeyColumns(Type keyType);
    }
}