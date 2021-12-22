namespace TildeSql {
    using System.Threading.Tasks;

    public interface IFutureSingleResult<TEntity, TKey> {
        ValueTask<TEntity> SingleAsync();
    }
}