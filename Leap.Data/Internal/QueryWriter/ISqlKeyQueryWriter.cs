namespace Leap.Data.Internal.QueryWriter {
    using Leap.Data.Queries;

    public interface ISqlKeyQueryWriter {
        void Write<TEntity, TKey>(KeyQuery<TEntity, TKey> query, Command command)
            where TEntity : class;
    }
}