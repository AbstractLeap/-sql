namespace TildeSql {
    public interface IChangeDetector {
        bool HasChanged(string json, object obj);
    }
}