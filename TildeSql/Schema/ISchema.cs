namespace TildeSql.Schema {
    using System.Collections.Generic;

    public interface ISchema {
        IEnumerable<Collection> All();

        Collection GetCollection<TEntity>(string collectionName);

        Collection GetDefaultCollection<TEntity>();
    }
}