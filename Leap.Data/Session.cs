namespace Leap.Data {
    using System;
    using System.Threading;
    using System.Threading.Tasks;

    using Leap.Data.Internal;
    using Leap.Data.Operations;
    using Leap.Data.Schema;

    class Session : ISession {
        private readonly IConnectionFactory connectionFactory;

        private readonly ISchema schema;

        private readonly ISerializer serializer;

        private UnitOfWork.UnitOfWork unitOfWork;

        private IdentityMap.IdentityMap identityMap;

        private QueryEngine queryEngine;

        public Session(IConnectionFactory connectionFactory,
                       ISchema schema,
                       ISerializer serializer) {
            this.connectionFactory = connectionFactory;
            this.schema            = schema;
            this.serializer        = serializer;
            this.identityMap       = new IdentityMap.IdentityMap(this.schema);
            this.queryEngine       = new QueryEngine(connectionFactory, schema, this.identityMap, serializer);
        }

        public IQueryBuilder<TEntity> Get<TEntity>()
            where TEntity : class {
            return new QueryBuilder<TEntity>(this);
        }

        public Task SaveChangesAsync(CancellationToken cancellationToken = default) {
            throw new NotImplementedException();
        }

        public void Delete<TEntity>(TEntity entity)
            where TEntity : class {
            this.EnsureUnitOfWork();
            this.unitOfWork.Add(new DeleteOperation(entity));
            // TODO remove from identity map
        }

        public void Add<TEntity>(TEntity entity)
            where TEntity : class {
            this.EnsureUnitOfWork();
            this.unitOfWork.Add(new AddOperation(entity));
            // TODO add to identity map
        }

        void EnsureUnitOfWork() {
            if (this.unitOfWork == null) {
                this.unitOfWork = new UnitOfWork.UnitOfWork();
            }
        }
        
        public QueryEngine GetEngine() {
            return this.queryEngine;
        }
    }
}