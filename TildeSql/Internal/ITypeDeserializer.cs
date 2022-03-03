namespace TildeSql.Internal {
    using System;

    public interface ITypeDeserializer {
        public Type Deserialize(string typeName);
    }
}