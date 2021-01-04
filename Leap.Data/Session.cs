namespace Leap.Data {
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;

    using Fasterflect;

    using Leap.Data.IdentityMap;
    using Leap.Data.Internal;
    using Leap.Data.Operations;
    using Leap.Data.Schema;

    class Session : ISession {
        private readonly ISchema schema;

        private UnitOfWork.UnitOfWork unitOfWork;

        private readonly IdentityMap.IdentityMap identityMap;

        private readonly QueryEngine queryEngine;

        private readonly UpdateEngine updateEngine;

        private readonly ChangeTracker changeTracker;

        public Session(ISchema schema, ISerializer serializer, IQueryExecutor queryExecutor, IUpdateExecutor updateExecutor) {
            this.schema        = schema;
            this.identityMap   = new IdentityMap.IdentityMap(this.schema);
            this.queryEngine   = new QueryEngine(schema, this.identityMap, queryExecutor, serializer);
            this.updateEngine  = new UpdateEngine(updateExecutor);
            this.changeTracker = new ChangeTracker(serializer);
        }

        public IQueryBuilder<TEntity> Get<TEntity>()
            where TEntity : class {
            return new QueryBuilder<TEntity>(this);
        }

        public async Task SaveChangesAsync(CancellationToken cancellationToken = default) {
            // find changed entities, add those operations to the unit of work
            foreach (var tuple in this.identityMap.GetAll()) {
                if ((bool)this.changeTracker.CallMethod(new[] { tuple.Document.GetType().GetGenericArguments().First() }, nameof(ChangeTracker.HasEntityChanged), tuple.Document)) {
                    var updateOperation = (IOperation)typeof(UpdateOperation<,>).MakeGenericType(tuple.Document.GetType().GetGenericArguments().First(), tuple.Key.GetType())
                                                                                .CreateInstance(tuple.Document, tuple.Key);
                    this.unitOfWork.Add(updateOperation);
                }
            }

            // get sql to execute
            await this.updateEngine.ExecuteAsync(this.unitOfWork);

            // TODO reset states in identity map
            // TODO flush queryEngine
            // TODO reset queryEngine

            // instantiate new unit of work
            this.unitOfWork = new UnitOfWork.UnitOfWork();
        }

        public void Delete<TEntity>(TEntity entity)
            where TEntity : class {
            this.EnsureUnitOfWork();
            this.unitOfWork.Add(new DeleteOperation<TEntity>(entity));
            var table = this.schema.GetTable<TEntity>();
            var keyType = table.KeyType;
            var key = table.KeyExtractor.CallMethod(new[] { typeof(TEntity), keyType }, nameof(IKeyExtractor.Extract), entity);
            if (this.identityMap.TryGetValue<TEntity>(keyType, key, out var document)) {
                document.State = DocumentState.Deleted;
            }
        }

        public void Add<TEntity>(TEntity entity)
            where TEntity : class {
            this.EnsureUnitOfWork();
            this.unitOfWork.Add(new AddOperation<TEntity>(entity));
            var table = this.schema.GetTable<TEntity>();
            var keyType = table.KeyType;
            var key = table.KeyExtractor.CallMethod(new[] { typeof(TEntity), keyType }, nameof(IKeyExtractor.Extract), entity);
            this.identityMap.Add(keyType, key, new Document<TEntity>(null, entity) { State = DocumentState.New });
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