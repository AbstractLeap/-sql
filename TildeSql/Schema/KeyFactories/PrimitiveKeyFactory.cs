namespace TildeSql.Schema.KeyFactories {
    class PrimitiveKeyFactory : IKeyFactory {
        public object Create(object[] row) {
            return row[0];
        }
    }
}