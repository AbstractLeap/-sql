namespace Leap.Data.Internal.QueryWriter {
    using Leap.Data.Queries;

    public interface ISqlMultipleKeyQueryWriter {
        void Write<TEntity, TKey>(MultipleKeyQuery<TEntity, TKey> query, Command command)
            where TEntity : class;
    }
}