namespace Leap.Data.Internal.QueryWriter {
    using Leap.Data.Queries;

    interface ISqlQueryWriter {
        void Write(IQuery query, Command command);
    }
}