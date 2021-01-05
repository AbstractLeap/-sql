namespace Leap.Data.Internal.UpdateWriter {
    using Leap.Data.Operations;

    public interface ISqlUpdateWriter {
        void Write(IOperation operation, Command command);
    }
}