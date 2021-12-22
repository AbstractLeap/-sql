namespace TildeSql.Events {
    using System.Threading.Tasks;

    public interface ISaveChangesEventListener {
        ValueTask OnBeforeSaveChangesAsync(ISession session);
    }
}