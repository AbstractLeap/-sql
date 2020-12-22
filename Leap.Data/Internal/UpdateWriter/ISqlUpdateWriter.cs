namespace Leap.Data.Internal.UpdateWriter {
    using Leap.Data.Operations;

    interface ISqlUpdateWriter {
        void Write(IOperation operation, Command command);
    }
}