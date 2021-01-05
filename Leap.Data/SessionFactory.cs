namespace Leap.Data {
    using Leap.Data.Internal;
    using Leap.Data.Schema;
    using Leap.Data.Serialization;

    class SessionFactory : ISessionFactory {
        private readonly ISchema schema;

        private readonly ISerializer serializer;

        private readonly IQueryExecutor queryExecutor;

        private readonly IUpdateExecutor updateExecutor;

        public SessionFactory(ISchema schema, ISerializer serializer, IQueryExecutor queryExecutor, IUpdateExecutor updateExecutor) {
            this.schema         = schema;
            this.serializer     = serializer;
            this.queryExecutor  = queryExecutor;
            this.updateExecutor = updateExecutor;
        }

        public ISession StartSession() {
            return new Session(this.schema, this.serializer, this.queryExecutor, this.updateExecutor);
        }
    }
}