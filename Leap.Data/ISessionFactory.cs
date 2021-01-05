namespace Leap.Data {
    public interface ISessionFactory {
        ISession StartSession();
    }
}