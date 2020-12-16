namespace Leap.Data {
    using System;
    using System.Threading;
    using System.Threading.Tasks;

    class Session : ISession {
        public IQueryBuilder<TEntity> Get<TEntity>() {
            return new QueryBuilder<TEntity>();
        }

        public Task SaveChangesAsync(CancellationToken cancellationToken = default) {
            throw new NotImplementedException();
        }

        public void Delete<TEntity>(TEntity entity) {
            this.EnsureUnitOfWork();
            this.unitOfWork.Add(new DeleteOperation(entity));
        }

        public void Add<TEntity>(TEntity entity) {
            this.EnsureUnitOfWork();
            this.unitOfWork.Add(new AddOperation(entity));
        }

        private UnitOfWork unitOfWork;

        void EnsureUnitOfWork() {
            if (this.unitOfWork == null) {
                this.unitOfWork = new UnitOfWork();
            }
        }
    }
}