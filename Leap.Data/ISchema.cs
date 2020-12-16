namespace Leap.Data {
    internal interface ISchema {
        Table GetTable<TEntity>();
    }
}