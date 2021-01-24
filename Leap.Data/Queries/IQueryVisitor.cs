namespace Leap.Data.Queries
{
    public interface IQueryVisitor {
        void VisitEntityQuery<TEntity>(EntityQuery<TEntity> entityQuery) where TEntity : class;

        void VisitKeyQuery<TEntity, TKey>(KeyQuery<TEntity, TKey> keyQuery) where TEntity : class;

        void VisitMultipleKeyQuery<TEntity, TKey>(MultipleKeyQuery<TEntity, TKey> multipleKeyQuery) where TEntity : class;
    }
}