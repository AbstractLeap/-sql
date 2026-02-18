namespace TildeSql.Serialization {
    public class DefaultChangeDetector : IChangeDetector {
        private readonly ISerializer serializer;

        public DefaultChangeDetector(ISerializer serializer) {
            this.serializer = serializer;
        }

        public bool HasChanged(string json, object obj) {
            return !JsonEquality.JsonEquals(json, this.serializer.Serialize(obj));
        }
    }
}