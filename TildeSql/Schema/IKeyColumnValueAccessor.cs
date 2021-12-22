namespace TildeSql.Schema {
    interface IKeyColumnValueAccessor {
        object GetValue<TKey>(TKey key);
    }
}