namespace TildeSql.SqlServer {
    using System;
    using System.Collections.Generic;
    using System.Data;

    public static class DbTypeSqlMap {
        private static readonly IDictionary<DbType, string> map = new Dictionary<DbType, string> { { DbType.Int32, "int" }, { DbType.Int64, "bigint" } };

        public static void AddMapping(DbType dbType, string dbTypeName) {
            if (map.ContainsKey(dbType)) {
                throw new Exception("Key already exists");
            }

            map[dbType] = dbTypeName;
        }

        public static bool TryGetValue(DbType dbType, out string sqlServerDataTypeName) {
            return map.TryGetValue(dbType, out sqlServerDataTypeName);
        }

        public static bool TryGetValue(Type type, out string sqlServerDataTypeName) {
            if (!DbTypeMap.TryGetValue(type, out var dbType)) {
                sqlServerDataTypeName = null;
                return false;
            }

            return map.TryGetValue(dbType, out sqlServerDataTypeName);
        }
    }
}

    