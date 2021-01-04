namespace Leap.Data.Schema {
    public interface ISchema {
        Table GetTable<TEntity>();
    }
}