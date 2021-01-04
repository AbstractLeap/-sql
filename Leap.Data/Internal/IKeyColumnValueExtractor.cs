namespace Leap.Data.Internal {
    using System.Collections.Generic;

    using Leap.Data.Schema;

    public interface IKeyColumnValueExtractor {
        IDictionary<Column, object> Extract<TEntity, TKey>(TKey key);
    }
}