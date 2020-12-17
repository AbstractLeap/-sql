namespace Leap.Data.Schema {
    internal interface ISchema {
        Table GetTable<TEntity>();
    }
}