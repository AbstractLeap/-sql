namespace Leap.Data.Internal {
    using System.Data;

    record ParameterInfo(string Name, object Value, ParameterDirection Direction, DbType? Type, int? Size);
}