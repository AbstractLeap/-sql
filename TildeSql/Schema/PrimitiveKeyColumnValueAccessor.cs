namespace TildeSql.Schema {
    class PrimitiveKeyColumnValueAccessor : IKeyColumnValueAccessor {
        public object GetValue<TKey>(TKey key) {
            return key;
        }
    }
}