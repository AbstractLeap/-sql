namespace TildeSql.Internal.QueryWriter {
    using TildeSql.Queries;

    public interface ISqlQueryWriter {
        void Write(IQuery query, Command command);
    }
}