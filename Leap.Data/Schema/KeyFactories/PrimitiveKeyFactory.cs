namespace Leap.Data.Schema.KeyFactories {
    class PrimitiveKeyFactory : IKeyFactory {
        public object Create(object[] row) {
            return row[0];
        }
    }
}