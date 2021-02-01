namespace Leap.Data {
    using System;

    using Leap.Data.Schema;

    public interface IEntityInspector<TEntity> {
        T GetColumnValue<T>(string columnName);
    }

    class EntityInspector<TEntity> : IEntityInspector<TEntity>
        where TEntity : class {
        private readonly Table table;

        private readonly UnitOfWork.UnitOfWork unitOfWork;

        private readonly TEntity entity;

        public EntityInspector(ISchema schema, UnitOfWork.UnitOfWork unitOfWork, TEntity entity) {
            this.table      = schema.GetDefaultTable<TEntity>();
            this.unitOfWork = unitOfWork;
            this.entity     = entity;
        }

        public EntityInspector(Table table, UnitOfWork.UnitOfWork unitOfWork, TEntity entity) {
            this.table      = table;
            this.unitOfWork = unitOfWork;
            this.entity     = entity;
        }

        public T GetColumnValue<T>(string columnName) {
            if (!this.unitOfWork.IsAttached(this.table, this.entity)) {
                throw new Exception($"The entity {this.entity} is not attached to this session");
            }

            var colIdx = table.GetColumnIndex(columnName);
            var obj = this.unitOfWork.GetRow(this.table, this.entity).Values[colIdx];
            if (!(obj is T typedObj)) {
                throw new Exception($"Unable to cast object to type {typeof(T)}");
            }

            return typedObj;
        }
    }
}