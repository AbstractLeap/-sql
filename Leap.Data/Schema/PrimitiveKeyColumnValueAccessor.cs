namespace Leap.Data.Schema {
    class PrimitiveKeyColumnValueAccessor : IKeyColumnValueAccessor {
        public object GetValue<TKey>(TKey key) {
            return key;
        }
    }
}