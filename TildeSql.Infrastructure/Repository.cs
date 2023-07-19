namespace TildeSql.Infrastructure {
    using TildeSql.Model;

    public class Repository<TEntity, TKey> : IRepository<TEntity, TKey>
        where TEntity : class {
        protected readonly ISession Session;

        protected string CollectionName;

        public Repository(ISession session) {
            this.Session = session;
        }

        public virtual ISingleEntityAccessor<TEntity, TKey> GetByIdAsync(TKey key, CancellationToken cancellationToken = default) {
            var futureResult = this.Session.Get<TEntity>(this.CollectionName).SingleFuture(key); // queues up the query for execution
            return new SingleEntityAccessor<TEntity, TKey>(futureResult);
        }

        public virtual IAsyncEnumerable<TEntity> All() {
            return this.Session.Get<TEntity>(this.CollectionName).Future();
        }

        public virtual void Add(TEntity entity) {
            this.Session.Add(entity, this.CollectionName);
        }

        public virtual void Remove(TEntity entity) {
            this.Session.Delete(entity, this.CollectionName);
        }
    }
}