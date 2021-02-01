namespace Leap.Data.Schema {
    public interface ISchema {
        Table GetTable(string collectionName);
        
        Table GetDefaultTable<TEntity>();
    }
}