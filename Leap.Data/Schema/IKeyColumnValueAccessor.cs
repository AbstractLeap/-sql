namespace Leap.Data.Schema {
    interface IKeyColumnValueAccessor {
        object GetValue<TKey>(TKey key);
    }
}