namespace Leap.Data.Schema {
    using System;
    using System.Collections.Generic;

    class DefaultSchemaConvention : INamingSchemaConvention, ICollectionNamingSchemaConvention, ISchemaNamingSchemaConvention, IKeyTypeSchemaConvention, IKeyColumnsSchemaConvention {
        public virtual string GetTableName(Type type) {
            return type.Name;
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

        public virtual string GetCollectionName(Type type) {
            return type.Name;
        }
    }
}