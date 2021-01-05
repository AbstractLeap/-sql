namespace Leap.Data.Schema {
    using System;
    using System.Collections.Generic;

    using Leap.Data.Schema.Columns;

    class DefaultSchemaConvention : ISchemaConvention {
        public virtual string GetTableName(Type type) {
            return type.Name; // TODO pluralize?
        }

        public virtual string GetSchemaName(string tableName) {
            return "dbo";
        }

        public virtual Type GetKeyType(string tableName, IEnumerable<Type> entityTypes) {
            return new DefaultKeyTypeExtractor().Extract(tableName, entityTypes);
        }

        public virtual IEnumerable<(Type Type, string Name)> GetKeyColumns(Type keyType) {
            return new DefaultKeyColumnExtractor().Extract(keyType);
        }
    }
}