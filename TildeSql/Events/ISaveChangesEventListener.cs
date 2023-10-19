namespace TildeSql.Events {
    using System.Threading.Tasks;

    public interface ISaveChangesEventListener : IInterceptor {
        ValueTask OnBeforeSaveChangesAsync(ISession session);
    }
}