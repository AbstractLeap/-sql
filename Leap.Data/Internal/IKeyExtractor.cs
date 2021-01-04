namespace Leap.Data.Internal {
    public interface IKeyExtractor {
        TKey Extract<TEntity, TKey>(TEntity entity);
    }
}