namespace TildeSql {
    using System.Data.Common;

    public interface IConnectionFactoryFactory {
        IConnectionFactory Get();
    }

    public interface IConnectionFactory {
        DbConnection Get();
    }
}