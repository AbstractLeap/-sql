namespace Leap.Data.Schema {
    using System;

    using Leap.Data.Schema.Columns;

    interface IKeyFactory {
        object Create(object[] row);
    }

    class PrimitiveKeyFactory : IKeyFactory {
        public object Create(object[] row) {
            return row[0];
        }
    }

    class TupleKeyFactory : IKeyFactory {
        public object Create(object[] row) {
            throw new NotImplementedException();
        }
    }

    class MultipleKeyFactory : IKeyFactory {
        private readonly KeyColumn[] keyColumns;

        public MultipleKeyFactory(KeyColumn[] keyColumns) {
            this.keyColumns = keyColumns;
        }

        public object Create(object[] row) {
            // there is only 1 keymember but possibly multiple columns under that
        }
    }
}