namespace Leap.Data {
    using System.Data.Common;

    internal interface IConnectionFactory {
        DbConnection Get();
    }
}