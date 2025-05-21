namespace TildeSql.Queries {
    interface ICountQuery {
        ICountAccessor CountAccessor { get; }
    }
}