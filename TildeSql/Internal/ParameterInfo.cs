namespace TildeSql.Internal {
    using System.Data;

    record ParameterInfo(string Name, object Value, ParameterDirection Direction, DbType? Type, int? Size);
}