namespace Leap.Data.Internal {
    using Leap.Data.Internal.ColumnValueFactories;
    using Leap.Data.Schema;
    using Leap.Data.Serialization;

    public class DatabaseRowFactory {
        private readonly ColumnValueFactoryFactory columnValueFactoryFactory;

        public DatabaseRowFactory(ISerializer serializer) {
            this.columnValueFactoryFactory = new ColumnValueFactoryFactory(serializer);
        }

        public DatabaseRow Create<TEntity, TKey>(Collection collection, TEntity entity) {
            var key = (TKey)collection.GetKey<TEntity, TKey>(entity);
            var values = new object[collection.Columns.Count];
            foreach (var keyColumn in collection.KeyColumns) {
                values[collection.GetColumnIndex(keyColumn.Name)] = collection.GetKeyColumnValue<TEntity, TKey>(key, keyColumn);
            }

            foreach (var nonKeyColumn in collection.NonKeyColumns) {
                values[collection.GetColumnIndex(nonKeyColumn.Name)] = this.columnValueFactoryFactory.GetFactory(nonKeyColumn).GetValue<TEntity, TKey>(nonKeyColumn, entity);
            }

            return new DatabaseRow(collection, values);
        }
    }
}