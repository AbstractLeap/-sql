namespace TildeSql.Schema.Conventions.Sql {
    using System;
    using System.Collections.Generic;

    public class SqlStorageSchemaConvention : IStorageSchemaConvention {
        private readonly SchemaBuilder builder;

        public SqlStorageSchemaConvention(SchemaBuilder builder) {
            this.builder = builder;
        }

        public ICollectionStorageSettings Configure(string collectionName, HashSet<Type> types) {
            var tableName = this.builder.GetConvention<ITableNamingSchemaConvention>().GetTableName(collectionName);
            var schemaName = this.builder.GetConvention<ISchemaNamingSchemaConvention>().GetSchemaName(tableName);
            var settings = new SqlCollectionStorageSettings { TableName = tableName, SchemaName = schemaName };
            return settings;
        }
    }
}