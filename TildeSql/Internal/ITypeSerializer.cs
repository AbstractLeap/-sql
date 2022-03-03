namespace TildeSql.Internal {
    using System;

    public interface ITypeSerializer {
        public string Serialize(Type type);

        public Type Deserialize(string typeName);
    }
}