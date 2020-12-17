namespace Leap.Data {
    using System.Threading.Tasks;

    public interface IFutureSingleResult<TEntity, TKey> {
        ValueTask<TEntity> SingleAsync();
    }
}