namespace Leap.Data.Internal {
    interface IKeyExtractor {
        TKey Extract<TEntity, TKey>(TEntity entity);
    }
}