namespace Leap.Data {
    using System;

    using Leap.Data.Schema;

    public interface IEntityInspector<TEntity> {
        T GetColumnValue<T>(string columnName);
    }

    class EntityInspector<TEntity> : IEntityInspector<TEntity>
        where TEntity : class {
        private readonly Collection collection;

        private readonly UnitOfWork.UnitOfWork unitOfWork;

        private readonly TEntity entity;

        public EntityInspector(ISchema schema, UnitOfWork.UnitOfWork unitOfWork, TEntity entity) {
            this.collection      = schema.GetDefaultCollection<TEntity>();
            this.unitOfWork = unitOfWork;
            this.entity     = entity;
        }

        public EntityInspector(Collection collection, UnitOfWork.UnitOfWork unitOfWork, TEntity entity) {
            this.collection      = collection;
            this.unitOfWork = unitOfWork;
            this.entity     = entity;
        }

        public T GetColumnValue<T>(string columnName) {
            if (!this.unitOfWork.IsAttached(this.collection, this.entity)) {
                throw new Exception($"The entity {this.entity} is not attached to this session");
            }

            var colIdx = this.collection.GetColumnIndex(columnName);
            var obj = this.unitOfWork.GetRow(this.collection, this.entity).Values[colIdx];
            if (!(obj is T typedObj)) {
                throw new Exception($"Unable to cast object to type {typeof(T)}");
            }

            return typedObj;
        }
    }
}