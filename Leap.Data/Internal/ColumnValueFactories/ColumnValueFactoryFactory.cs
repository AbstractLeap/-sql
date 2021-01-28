namespace Leap.Data.Internal.ColumnValueFactories {
    using System;

    using Leap.Data.Schema.Columns;
    using Leap.Data.Serialization;

    class ColumnValueFactoryFactory {
        private readonly ISerializer serializer;

        public ColumnValueFactoryFactory(ISerializer serializer) {
            this.serializer = serializer;
        }

        public IColumnValueFactory GetFactory(Column column) {
            if (column is KeyColumn) {
                return new KeyColumnValueFactory(column.Table);
            } else if (column is DocumentColumn) {
                return new DocumentColumnValueFactory(this.serializer);
            } else if (column is DocumentTypeColumn) {
                return new DocumentTypeColumnValueFactory();
            } else if (column is OptimisticConcurrencyColumn) {
                return new OptimisticConcurrencyColumnValueFactory();
            } else if (column.IsComputed) {
                return new NullColumnFactory();
            }

            throw new NotSupportedException();
        }
    }
}