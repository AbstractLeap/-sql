namespace Leap.Data.Schema {
    using System.Collections.Generic;

    public interface ISchema {
        IEnumerable<Table> All();

        Table GetTable(string collectionName);
        
        Table GetDefaultTable<TEntity>();
    }
}