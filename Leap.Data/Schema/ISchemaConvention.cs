namespace Leap.Data.Schema {
    using System;
    using System.Collections.Generic;

    public interface ISchemaConvention {
        string GetTableName(Type type);

        string GetSchemaName(string tableName);

        Type GetKeyType(string tableName, IEnumerable<Type> entityTypes);

        IEnumerable<Column> GetKeyColumns(Type keyType);
    }
}