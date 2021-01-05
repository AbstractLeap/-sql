namespace Leap.Data.Internal.QueryWriter {
    using Leap.Data.Queries;

    public interface ISqlQueryWriter {
        void Write(IQuery query, Command command);
    }
}