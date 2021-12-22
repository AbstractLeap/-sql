namespace TildeSql {
    public interface ISessionFactory {
        ISession StartSession();
    }
}