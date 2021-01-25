namespace Leap.Data.Internal.QueryWriter {
    using Leap.Data.Queries;

    public interface ISqlEntityQueryWriter {
        void Write<TEntity>(EntityQuery<TEntity> query, Command command)
            where TEntity : class;
    }
}