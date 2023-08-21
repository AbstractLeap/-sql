namespace TildeSql {
    using System.Data.Common;
    using System.Threading.Tasks;

    public interface IConnectionFactoryFactory {
        IConnectionFactory Get();
    }

    public interface IConnectionFactory {
        ValueTask<DbConnection> GetAsync();
    }
}