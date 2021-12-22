namespace TildeSql.Internal.QueryWriter {
    using TildeSql.Queries;

    public interface ISqlKeyQueryWriter {
        void Write<TEntity, TKey>(KeyQuery<TEntity, TKey> query, Command command)
            where TEntity : class;
    }
}