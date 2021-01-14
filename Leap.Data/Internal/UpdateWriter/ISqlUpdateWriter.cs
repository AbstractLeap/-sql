namespace Leap.Data.Internal.UpdateWriter {
    public interface ISqlUpdateWriter {
        void WriteInsert(DatabaseRow row, Command command);

        void WriteUpdate(DatabaseRow row, Command command);

        void WriteDelete(DatabaseRow row, Command command);
    }
}