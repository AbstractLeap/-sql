namespace Leap.Data {
    using System.Data.Common;

    public interface IConnectionFactory {
        DbConnection Get();
    }
}