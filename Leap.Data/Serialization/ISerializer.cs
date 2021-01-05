namespace Leap.Data.Serialization {
    using System;

    public interface ISerializer {
        string Serialize(object obj);

        object Deserialize(Type type, string json);
    }
}