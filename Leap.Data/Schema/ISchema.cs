namespace Leap.Data.Schema {
    using System.Collections.Generic;

    public interface ISchema {
        IEnumerable<Collection> All();

        Collection GetCollection(string collectionName);
        
        Collection GetDefaultCollection<TEntity>();
    }
}