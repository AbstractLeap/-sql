namespace TildeSql.Exceptions {
    using System;

    using TildeSql.Internal;

    [Serializable]
    public class OptimisticConcurrencyException : Exception {
        private DatabaseRow databaseRow;

        public DatabaseRow DatabaseRow {
            get => this.databaseRow;
            private set {
                this.databaseRow = value;

                base.Data[nameof(DatabaseRow.Collection.KeyType)] = value?.Collection?.KeyType;
                base.Data[nameof(DatabaseRow.Collection)]         = value?.Collection?.CollectionName;
                base.Data[nameof(DatabaseRow.Values)]             = value?.Values;
            }
        }

        public OptimisticConcurrencyException(DatabaseRow databaseRow) {
            this.DatabaseRow = databaseRow;
        }

        public OptimisticConcurrencyException(DatabaseRow databaseRow, string message)
            : base(message) {
            this.DatabaseRow = databaseRow;
        }

        public OptimisticConcurrencyException(DatabaseRow databaseRow, string message, Exception inner)
            : base(message, inner) {
            this.DatabaseRow = databaseRow;
        }
    }
}