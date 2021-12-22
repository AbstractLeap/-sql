namespace TildeSql.Internal.QueryWriter {
    using TildeSql.Queries;

    public interface ISqlMultipleKeyQueryWriter {
        void Write<TEntity, TKey>(MultipleKeyQuery<TEntity, TKey> query, Command command)
            where TEntity : class;
    }
}