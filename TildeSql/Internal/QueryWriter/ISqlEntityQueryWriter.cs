namespace TildeSql.Internal.QueryWriter {
    using TildeSql.Queries;

    public interface ISqlEntityQueryWriter {
        void Write<TEntity>(EntityQuery<TEntity> query, Command command)
            where TEntity : class;
    }
}