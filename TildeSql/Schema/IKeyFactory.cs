namespace TildeSql.Schema {
    public interface IKeyFactory {
        object Create(object[] row);
    }
}