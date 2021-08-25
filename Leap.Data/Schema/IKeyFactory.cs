namespace Leap.Data.Schema {
    public interface IKeyFactory {
        object Create(object[] row);
    }
}