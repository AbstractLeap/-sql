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

    [Serializable]
    public class OptimisticUpdateException : Exception {
        private DatabaseRow newDatabaseRow;

        private DatabaseRow oldDatabaseRow;

        public DatabaseRow NewDatabaseRow {
            get => this.newDatabaseRow;
            private set {
                this.newDatabaseRow = value;

                base.Data["New" + nameof(DatabaseRow.Collection.KeyType)] = value?.Collection?.KeyType;
                base.Data["New" + nameof(DatabaseRow.Collection)]         = value?.Collection?.CollectionName;
                base.Data["New" + nameof(DatabaseRow.Values)]             = value?.Values;
            }
        }

        public DatabaseRow OldDatabaseRow {
            get => this.oldDatabaseRow;
            private set {
                this.oldDatabaseRow = value;

                base.Data["Old" + nameof(DatabaseRow.Collection.KeyType)] = value?.Collection?.KeyType;
                base.Data["Old" + nameof(DatabaseRow.Collection)]         = value?.Collection?.CollectionName;
                base.Data["Old" + nameof(DatabaseRow.Values)]             = value?.Values;
            }
        }

        public OptimisticUpdateException(string message, DatabaseRow newRow, DatabaseRow oldRow)
            : base(message) {
            this.NewDatabaseRow = newRow;
            this.OldDatabaseRow = oldRow;
        }
    }

    [Serializable]
    public class OptimisticDeleteException : Exception {
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

        public OptimisticDeleteException(string message, DatabaseRow deletedRow)
            : base(message) {
            this.DatabaseRow = deletedRow;
        }
    }
}